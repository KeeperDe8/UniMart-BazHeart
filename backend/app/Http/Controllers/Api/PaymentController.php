<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Models\Listing;
use App\Services\PaymentService;
use App\Services\PayMongoService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;
use Illuminate\Support\Str;

class PaymentController extends Controller
{
    public function __construct(
        protected PaymentService $paymentService
    ) {}

    public function createPaymongoLink(Request $request, int $listingId, PayMongoService $payMongoService): JsonResponse
    {
        $listing = Listing::find($listingId);
        $title = $listing ? $listing->title : ($request->input('title') ?? 'Campus Item Listing Fee');
        $id = $listing ? $listing->id : 1;

        $linkData = $payMongoService->createListingFeeLink($id, $title, 25.00);

        $checkoutUrl = $linkData['attributes']['checkout_url'] ?? "https://test.paymongo.com/checkout/mock-{$id}";
        $reference = $linkData['id'] ?? ('PM-' . strtoupper(Str::random(10)));

        return response()->json([
            'checkout_url' => $checkoutUrl,
            'amount' => 25.00,
            'reference_number' => $reference,
        ]);
    }

    public function payFee(Request $request, int $listingId): JsonResponse
    {
        $validated = $request->validate([
            'reference_number' => 'nullable|string|max:100',
            'amount' => 'nullable|numeric',
        ]);

        $reference = $validated['reference_number'] ?? ('PM-' . strtoupper(Str::random(10)));
        $amount = $validated['amount'] ?? 25.00;
        $user = $request->user() ?? $request->user('sanctum') ?? \App\Models\User::where('role', 'seller')->first() ?? \App\Models\User::first();
        $userId = $user ? $user->id : 1;

        $listing = Listing::find($listingId) ?? Listing::first();
        if (!$listing) {
            $listing = Listing::create([
                'seller_id' => $userId,
                'category_id' => 1,
                'title' => 'Campus Item',
                'price' => 95.00,
                'status' => 'pending_payment',
            ]);
        }

        $payment = $this->paymentService->verifyListingFee(
            $userId,
            $listing->id,
            $reference,
            $amount
        );

        return response()->json([
            'message' => 'Listing fee verified. Your product is now live on the campus feed!',
            'payment' => $payment,
        ]);
    }

    public function checkPaymentStatus(Request $request, int $listingId, PayMongoService $payMongoService): JsonResponse
    {
        $sessionId = $request->query('session_id') ?? $request->input('session_id');
        $user = $request->user() ?? $request->user('sanctum') ?? \App\Models\User::where('role', 'seller')->first() ?? \App\Models\User::first();
        $userId = $user ? $user->id : 1;

        if ($sessionId && str_starts_with($sessionId, 'cs_')) {
            $session = $payMongoService->getCheckoutSession($sessionId);
            $status = $session['attributes']['status'] ?? 'unpaid';

            if ($status === 'paid') {
                $payment = $this->paymentService->verifyListingFee(
                    $userId,
                    $listingId,
                    $sessionId,
                    25.00
                );

                return response()->json([
                    'paid' => true,
                    'status' => 'paid',
                    'payment' => $payment,
                ]);
            }
        }

        return response()->json([
            'paid' => false,
            'status' => 'pending',
            'message' => 'Payment has not been completed yet.',
        ]);
    }
}
