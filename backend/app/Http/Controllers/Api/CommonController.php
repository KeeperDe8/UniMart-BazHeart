<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\CampusLocation;
use App\Models\Category;
use App\Models\User;
use Illuminate\Http\JsonResponse;

class CommonController extends Controller
{
    public function categories(): JsonResponse
    {
        return response()->json([
            'categories' => Category::all(),
        ]);
    }

    public function locations(): JsonResponse
    {
        return response()->json([
            'locations' => CampusLocation::all(),
        ]);
    }

    public function publicSellerShop(int $id): JsonResponse
    {
        $seller = User::with(['listings' => function ($q) {
            $q->where('status', 'active')->with('primaryImage');
        }, 'sellingSchedules.location'])
        ->where('id', $id)
        ->firstOrFail();

        return response()->json([
            'seller' => [
                'id' => $seller->id,
                'name' => $seller->name,
                'shop_name' => $seller->seller_shop_name,
                'bio' => $seller->seller_bio,
                'avatar_url' => $seller->avatar_url,
                'preferred_meetup_area' => $seller->preferred_meetup_area,
                'is_verified' => $seller->is_verified,
            ],
            'listings' => $seller->listings,
            'schedules' => $seller->sellingSchedules,
        ]);
    }
}
