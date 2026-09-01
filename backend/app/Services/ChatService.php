<?php

namespace App\Services;

use App\Models\Conversation;
use App\Models\ConversationParticipant;
use App\Models\MeetupAppointment;
use App\Models\Message;
use App\Models\Notification;
use App\Models\User;
use Illuminate\Database\Eloquent\Collection;
use Illuminate\Support\Str;

class ChatService
{
    public function getConversations(int $userId): Collection
    {
        return Conversation::whereHas('participants', function ($q) use ($userId) {
            $q->where('user_id', $userId);
        })
        ->with([
            'listing:id,title,price',
            'latestMessage',
            'participants' => function ($q) use ($userId) {
                $q->where('user_id', '!=', $userId)->select('users.id', 'name', 'avatar_url', 'seller_shop_name');
            }
        ])
        ->orderBy('last_message_at', 'desc')
        ->get()
        ->map(function ($conv) use ($userId) {
            $otherUser = $conv->participants->first();
            $myPivot = ConversationParticipant::where('conversation_id', $conv->id)
                ->where('user_id', $userId)
                ->first();

            $unreadCount = Message::where('conversation_id', $conv->id)
                ->where('sender_id', '!=', $userId)
                ->where(function ($q) use ($myPivot) {
                    if ($myPivot?->last_read_at) {
                        $q->where('created_at', '>', $myPivot->last_read_at);
                    }
                })
                ->count();

            $conv->other_user = $otherUser;
            $conv->unread_count = $unreadCount;
            return $conv;
        });
    }

    public function getMessages(int $conversationId, int $userId): array
    {
        $conversation = Conversation::with(['listing'])->findOrFail($conversationId);

        // Mark as read
        ConversationParticipant::where('conversation_id', $conversationId)
            ->where('user_id', $userId)
            ->update(['last_read_at' => now()]);

        $messages = Message::with(['sender:id,name,avatar_url', 'meetupAppointment.location'])
            ->where('conversation_id', $conversationId)
            ->orderBy('created_at', 'asc')
            ->get();

        return [
            'conversation' => $conversation,
            'messages' => $messages,
        ];
    }

    public function sendMessage(int $senderId, array $data): Message
    {
        $conversationId = $data['conversation_id'] ?? null;

        if (!$conversationId && !empty($data['recipient_id'])) {
            $conversation = $this->getOrCreateConversation($senderId, $data['recipient_id'], $data['listing_id'] ?? null);
            $conversationId = $conversation->id;
        }

        $type = $data['message_type'] ?? 'text';

        $message = Message::create([
            'conversation_id' => $conversationId,
            'sender_id' => $senderId,
            'message_type' => $type,
            'body' => $data['body'],
            'is_read' => false,
        ]);

        if ($type === 'meetup_card' && !empty($data['meetup'])) {
            MeetupAppointment::create([
                'message_id' => $message->id,
                'location_id' => $data['meetup']['location_id'] ?? 1,
                'scheduled_datetime' => $data['meetup']['scheduled_datetime'] ?? now()->addDay(),
                'status' => 'confirmed',
                'notes' => $data['meetup']['notes'] ?? 'Please be on time at the meetup hotspot.',
            ]);
        }

        Conversation::where('id', $conversationId)->update(['last_message_at' => now()]);

        // In-app Notification for recipient
        $recipientId = ConversationParticipant::where('conversation_id', $conversationId)
            ->where('user_id', '!=', $senderId)
            ->value('user_id');

        if ($recipientId) {
            $sender = User::find($senderId);
            Notification::create([
                'id' => (string) Str::uuid(),
                'user_id' => $recipientId,
                'type' => 'NewMessage',
                'title' => '💬 New Message from ' . ($sender?->name ?? 'Student'),
                'body' => Str::limit($data['body'], 80),
                'data' => [
                    'conversation_id' => $conversationId,
                    'sender_id' => $senderId,
                ],
            ]);
        }

        return $message->load(['sender', 'meetupAppointment.location']);
    }

    public function getOrCreateConversation(int $user1Id, int $user2Id, ?int $listingId = null): Conversation
    {
        $existing = Conversation::whereHas('participants', function ($q) use ($user1Id) {
            $q->where('user_id', $user1Id);
        })->whereHas('participants', function ($q) use ($user2Id) {
            $q->where('user_id', $user2Id);
        })->first();

        if ($existing) {
            return $existing;
        }

        $conversation = Conversation::create([
            'listing_id' => $listingId,
            'last_message_at' => now(),
        ]);

        ConversationParticipant::create(['conversation_id' => $conversation->id, 'user_id' => $user1Id, 'last_read_at' => now()]);
        ConversationParticipant::create(['conversation_id' => $conversation->id, 'user_id' => $user2Id, 'last_read_at' => null]);

        return $conversation;
    }
}
