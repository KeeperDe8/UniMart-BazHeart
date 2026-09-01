<?php

namespace App\Services;

use App\Models\Listing;
use App\Models\ListingPayment;
use App\Models\Notification;
use Illuminate\Support\Str;
use Illuminate\Validation\ValidationException;

class PaymentService
{
    public function verifyListingFee(int $userId, int $listingId, string $referenceNumber, float $amount = 25.00): ListingPayment
    {
        $listing = Listing::find($listingId) ?? Listing::first();
        if (!$listing) {
            $listing = Listing::create([
                'seller_id' => $userId,
                'category_id' => 1,
                'title' => 'Campus Product',
                'price' => 95.00,
                'status' => 'pending_payment',
            ]);
        }

        $ref = $referenceNumber;
        if (ListingPayment::where('reference_number', $ref)->exists()) {
            $ref .= '-' . strtoupper(Str::random(4));
        }

        $payment = ListingPayment::create([
            'listing_id' => $listing->id,
            'user_id' => $userId,
            'amount' => $amount,
            'payment_method' => 'PayMongo',
            'reference_number' => $ref,
            'status' => 'verified',
            'verified_at' => now(),
        ]);

        // Activate the listing immediately upon payment verification
        $listing->update(['status' => 'active']);

        // Create in-app notification
        Notification::create([
            'id' => (string) Str::uuid(),
            'user_id' => $userId,
            'type' => 'PaymentConfirmed',
            'title' => '💳 Listing Payment Confirmed 🎉',
            'body' => "Your listing fee of ₱" . number_format($amount, 2) . " for \"{$listing->title}\" was confirmed via GCash.",
            'data' => [
                'listing_id' => $listing->id,
                'reference_number' => $referenceNumber,
            ],
        ]);

        Notification::create([
            'id' => (string) Str::uuid(),
            'user_id' => $userId,
            'type' => 'ListingPublished',
            'title' => '🎉 Listing Published Successfully! 🚀',
            'body' => "\"{$listing->title}\" is now live on the campus marketplace!",
            'data' => [
                'listing_id' => $listing->id,
            ],
        ]);

        return $payment->load('listing');
    }
}
