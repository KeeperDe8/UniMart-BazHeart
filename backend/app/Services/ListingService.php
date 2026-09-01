<?php

namespace App\Services;

use App\Models\Listing;
use App\Models\ListingImage;
use App\Models\SellingSchedule;
use Illuminate\Database\Eloquent\Collection;
use Illuminate\Pagination\LengthAwarePaginator;

class ListingService
{
    public function getFeed(array $filters = []): Collection|LengthAwarePaginator
    {
        $query = Listing::with(['seller:id,name,seller_shop_name,avatar_url', 'category', 'location', 'primaryImage', 'images', 'schedules.location'])
            ->where('status', 'active');

        if (!empty($filters['category_id'])) {
            $query->where('category_id', $filters['category_id']);
        }

        if (!empty($filters['category_slug'])) {
            $query->whereHas('category', function ($q) use ($filters) {
                $q->where('slug', $filters['category_slug']);
            });
        }

        if (!empty($filters['search'])) {
            $search = $filters['search'];
            $query->where(function ($q) use ($search) {
                $q->where('title', 'like', "%{$search}%")
                  ->orWhere('description', 'like', "%{$search}%")
                  ->orWhereHas('seller', function ($sq) use ($search) {
                      $sq->where('name', 'like', "%{$search}%")
                         ->orWhere('seller_shop_name', 'like', "%{$search}%");
                  });
            });
        }

        return $query->latest()->get();
    }

    public function getListingDetails(int $id): Listing
    {
        return Listing::with(['seller', 'category', 'location', 'images', 'schedules.location'])
            ->findOrFail($id);
    }

    public function createListing(int $sellerId, array $data): Listing
    {
        $listing = Listing::create([
            'seller_id' => $sellerId,
            'category_id' => $data['category_id'] ?? 1,
            'title' => $data['title'],
            'description' => $data['description'] ?? null,
            'price' => $data['price'],
            'stock_quantity' => $data['stock_quantity'] ?? 1,
            'item_condition' => $data['item_condition'] ?? 'Freshly Prepared / Baked',
            'default_location_id' => $data['default_location_id'] ?? null,
            'pickup_instructions' => $data['pickup_instructions'] ?? null,
            'status' => 'pending_payment', // Gate: requires GCash listing fee
        ]);

        if (!empty($data['image_path'])) {
            ListingImage::create([
                'listing_id' => $listing->id,
                'image_path' => $data['image_path'],
                'is_primary' => true,
            ]);
        }

        if (!empty($data['schedules']) && is_array($data['schedules'])) {
            foreach ($data['schedules'] as $slot) {
                SellingSchedule::create([
                    'user_id' => $sellerId,
                    'listing_id' => $listing->id,
                    'day_of_week' => $slot['day_of_week'] ?? 'Monday',
                    'time_window' => $slot['time_window'] ?? '10:00 AM – 2:00 PM',
                    'location_id' => $slot['location_id'] ?? $listing->default_location_id,
                ]);
            }
        }

        return $listing->load(['seller', 'category', 'images', 'schedules']);
    }

    public function getSellerListings(int $sellerId, ?string $status = null): Collection
    {
        $query = Listing::with(['category', 'primaryImage', 'schedules'])
            ->where('seller_id', $sellerId);

        if ($status && in_array($status, ['active', 'pending_payment', 'sold_out'])) {
            $query->where('status', $status);
        }

        return $query->latest()->get();
    }
}
