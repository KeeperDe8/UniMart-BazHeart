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
}
