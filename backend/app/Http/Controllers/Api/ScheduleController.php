<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Services\ScheduleService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class ScheduleController extends Controller
{
    public function __construct(
        protected ScheduleService $scheduleService
    ) {}

    public function index(Request $request): JsonResponse
    {
        $schedules = $this->scheduleService->getSellerSchedules($request->user()->id);

        return response()->json([
            'schedules' => $schedules,
        ]);
    }

    public function store(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'day_of_week' => 'required|in:Monday,Tuesday,Wednesday,Thursday,Friday,Saturday,Sunday',
            'time_window' => 'required|string|max:60',
            'location_id' => 'nullable|exists:campus_locations,id',
            'listing_id' => 'nullable|exists:listings,id',
        ]);

        $schedule = $this->scheduleService->addSchedule($request->user()->id, $validated);

        return response()->json([
            'message' => 'Selling schedule slot added',
            'schedule' => $schedule,
        ], 201);
    }

    public function destroy(Request $request, int $id): JsonResponse
    {
        $this->scheduleService->removeSchedule($request->user()->id, $id);

        return response()->json([
            'message' => 'Schedule slot removed',
        ]);
    }
}
