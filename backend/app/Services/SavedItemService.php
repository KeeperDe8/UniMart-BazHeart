<?php

namespace App\Services;

use App\Models\SavedItem;
use Illuminate\Database\Eloquent\Collection;

class SavedItemService
{
    public function getSavedItems(int $userId): Collection
    {
        return SavedItem::with(['listing.seller', 'listing.primaryImage', 'listing.category'])
            ->where('user_id', $userId)
            ->latest()
            ->get()
            ->pluck('listing');
    }

    public function toggleSaved(int $userId, int $listingId): bool
    {
        $existing = SavedItem::where('user_id', $userId)->where('listing_id', $listingId)->first();

        if ($existing) {
            $existing->delete();
            return false; // un-saved
        }

        SavedItem::create(['user_id' => $userId, 'listing_id' => $listingId]);
        return true; // saved
    }
}
