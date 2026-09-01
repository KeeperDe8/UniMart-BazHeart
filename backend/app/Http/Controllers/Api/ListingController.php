<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Services\ListingService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class ListingController extends Controller
{
    public function __construct(
        protected ListingService $listingService
    ) {}

    public function index(Request $request): JsonResponse
    {
        $listings = $this->listingService->getFeed($request->only(['category_id', 'category_slug', 'search']));

        return response()->json([
            'listings' => $listings,
        ]);
    }

    public function show(int $id): JsonResponse
    {
        $listing = $this->listingService->getListingDetails($id);

        return response()->json([
            'listing' => $listing,
        ]);
    }

    public function store(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'title' => 'required|string|max:150',
            'category_id' => 'nullable|integer',
            'price' => 'required|numeric|min:1',
            'stock_quantity' => 'nullable|integer|min:1',
            'item_condition' => 'nullable|string|max:50',
            'description' => 'nullable|string',
            'default_location_id' => 'nullable|integer',
            'pickup_instructions' => 'nullable|string',
            'image_path' => 'nullable|string',
            'schedules' => 'nullable|array',
        ]);

        $user = $request->user() ?? $request->user('sanctum');
        $userId = $user ? $user->id : ($request->input('seller_id') ? (int)$request->input('seller_id') : 1);
        $validated['category_id'] = $validated['category_id'] ?? 1;
        $validated['stock_quantity'] = $validated['stock_quantity'] ?? 1;

        $listing = $this->listingService->createListing($userId, $validated);

        return response()->json([
            'message' => 'Listing draft created. Listing fee required to publish.',
            'listing' => $listing,
        ], 201);
    }

    public function mySellerListings(Request $request): JsonResponse
    {
        $status = $request->query('status');
        $user = $request->user() ?? $request->user('sanctum');
        $userId = $user ? $user->id : ($request->query('seller_id') ? (int)$request->query('seller_id') : 1);
        $listings = $this->listingService->getSellerListings($userId, $status);

        return response()->json([
            'listings' => $listings,
        ]);
    }
}
