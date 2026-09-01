<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Services\ChatService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class ChatController extends Controller
{
    public function __construct(
        protected ChatService $chatService
    ) {}

    public function conversations(Request $request): JsonResponse
    {
        $user = $request->user() ?? $request->user('sanctum');
        $userId = $user ? $user->id : ($request->query('user_id') ? (int)$request->query('user_id') : 1);
        $conversations = $this->chatService->getConversations($userId);

        return response()->json([
            'conversations' => $conversations,
        ]);
    }

    public function messages(Request $request, int $conversationId): JsonResponse
    {
        $user = $request->user() ?? $request->user('sanctum');
        $userId = $user ? $user->id : ($request->query('user_id') ? (int)$request->query('user_id') : 1);
        $data = $this->chatService->getMessages($conversationId, $userId);

        return response()->json($data);
    }

    public function sendMessage(Request $request): JsonResponse
    {
        $user = $request->user() ?? $request->user('sanctum');
        $userId = $user ? $user->id : ($request->input('sender_id') ? (int)$request->input('sender_id') : 1);
        $validated = $request->validate([
            'conversation_id' => 'nullable|exists:conversations,id',
            'recipient_id' => 'nullable|exists:users,id',
            'listing_id' => 'nullable|exists:listings,id',
            'message_type' => 'nullable|in:text,image,meetup_card',
            'body' => 'required|string',
            'meetup' => 'nullable|array',
            'meetup.location_id' => 'nullable|exists:campus_locations,id',
            'meetup.scheduled_datetime' => 'nullable|string',
            'meetup.notes' => 'nullable|string',
        ]);

        $message = $this->chatService->sendMessage($userId, $validated);

        return response()->json([
            'message' => $message,
        ], 201);
    }

    public function getOrCreate(Request $request): JsonResponse
    {
        $user = $request->user() ?? $request->user('sanctum');
        $userId = $user ? $user->id : ($request->input('user_id') ? (int)$request->input('user_id') : 1);

        $recipientId = (int)$request->input('recipient_id');
        $sellerName = $request->input('seller_name');
        $listingId = $request->input('listing_id') ? (int)$request->input('listing_id') : null;

        if (!$recipientId || $recipientId === $userId) {
            if (!empty($sellerName)) {
                $cleanName = trim(str_replace('@', '', $sellerName));
                $foundUser = \App\Models\User::where('name', 'like', "%{$cleanName}%")
                    ->orWhere('seller_shop_name', 'like', "%{$cleanName}%")
                    ->orWhere('email', 'like', "%{$cleanName}%")
                    ->first();
                if ($foundUser && $foundUser->id !== $userId) {
                    $recipientId = $foundUser->id;
                }
            }
        }

        if (!$recipientId || $recipientId === $userId) {
            $recipientId = $userId === 1 ? 2 : 1;
        }

        $conversation = $this->chatService->getOrCreateConversation($userId, $recipientId, $listingId);
        $data = $this->chatService->getMessages($conversation->id, $userId);

        return response()->json([
            'conversation_id' => $conversation->id,
            'recipient_id' => $recipientId,
            'messages' => $data['messages'],
        ]);
    }
}
