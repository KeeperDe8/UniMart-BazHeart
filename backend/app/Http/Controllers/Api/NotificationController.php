<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Services\NotificationService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class NotificationController extends Controller
{
    public function __construct(
        protected NotificationService $notificationService
    ) {}

    public function index(Request $request): JsonResponse
    {
        $userId = $request->user()?->id ?? \App\Models\User::first()?->id ?? 1;
        $notifications = $this->notificationService->getNotifications($userId);

        return response()->json([
            'notifications' => $notifications,
        ]);
    }

    public function markAllAsRead(Request $request): JsonResponse
    {
        $userId = $request->user()?->id ?? \App\Models\User::first()?->id ?? 1;
        $this->notificationService->markAllAsRead($userId);

        return response()->json([
            'message' => 'All notifications marked as read',
        ]);
    }

    public function registerDeviceToken(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'device_token' => 'required|string',
            'device_type' => 'nullable|in:android,ios,windows',
        ]);

        $token = $this->notificationService->registerDeviceToken(
            $request->user()->id,
            $validated['device_token'],
            $validated['device_type'] ?? 'android'
        );

        return response()->json([
            'message' => 'Device push token registered successfully',
            'device_token' => $token,
        ]);
    }
}
