<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Services\SavedItemService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class SavedItemController extends Controller
{
    public function __construct(
        protected SavedItemService $savedItemService
    ) {}

    public function index(Request $request): JsonResponse
    {
        $items = $this->savedItemService->getSavedItems($request->user()->id);

        return response()->json([
            'saved_items' => $items,
        ]);
    }

    public function toggle(Request $request, int $listingId): JsonResponse
    {
        $isSaved = $this->savedItemService->toggleSaved($request->user()->id, $listingId);

        return response()->json([
            'is_saved' => $isSaved,
            'message' => $isSaved ? 'Item saved to wishlist' : 'Item removed from wishlist',
        ]);
    }
}
