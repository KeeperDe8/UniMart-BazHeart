<?php

namespace App\Services;

use App\Models\SellingSchedule;
use Illuminate\Database\Eloquent\Collection;

class ScheduleService
{
    public function getSellerSchedules(int $userId): Collection
    {
        return SellingSchedule::with('location')
            ->where('user_id', $userId)
            ->get();
    }

    public function addSchedule(int $userId, array $data): SellingSchedule
    {
        return SellingSchedule::create([
            'user_id' => $userId,
            'listing_id' => $data['listing_id'] ?? null,
            'day_of_week' => $data['day_of_week'],
            'time_window' => $data['time_window'],
            'location_id' => $data['location_id'] ?? 1,
        ])->load('location');
    }

    public function removeSchedule(int $userId, int $scheduleId): bool
    {
        $schedule = SellingSchedule::where('id', $scheduleId)
            ->where('user_id', $userId)
            ->firstOrFail();

        return (bool) $schedule->delete();
    }
}
