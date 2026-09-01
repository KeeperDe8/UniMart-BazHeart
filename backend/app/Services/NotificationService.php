<?php

namespace App\Services;

use App\Models\DeviceToken;
use App\Models\Notification;
use App\Models\SavedItem;
use Illuminate\Database\Eloquent\Collection;

class NotificationService
{
    public function getNotifications(int $userId): Collection
    {
        return Notification::where('user_id', $userId)
            ->latest()
            ->get();
    }

    public function markAllAsRead(int $userId): void
    {
        Notification::where('user_id', $userId)
            ->whereNull('read_at')
            ->update(['read_at' => now()]);
    }

    public function registerDeviceToken(int $userId, string $token, string $type = 'android'): DeviceToken
    {
        return DeviceToken::updateOrCreate(
            ['user_id' => $userId, 'device_token' => $token],
            ['device_type' => $type, 'is_active' => true, 'last_used_at' => now()]
        );
    }
}
