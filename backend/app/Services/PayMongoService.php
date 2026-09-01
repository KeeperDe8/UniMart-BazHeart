<?php

namespace App\Services;

use Illuminate\Support\Facades\Http;

class PayMongoService
{
    private string $secretKey;
    private string $baseUrl = 'https://api.paymongo.com/v1';

    public function __construct()
    {
        $this->secretKey = config('services.paymongo.secret_key') ?? '';
    }

    /**
     * Create a PayMongo Checkout Session for a listing fee
     */
    public function createListingFeeLink(int $listingId, string $title, float $amount = 25.00): ?array
    {
        $response = Http::withBasicAuth($this->secretKey, '')
            ->post("{$this->baseUrl}/checkout_sessions", [
                'data' => [
                    'attributes' => [
                        'send_email_receipt' => false,
                        'show_description' => true,
                        'show_line_items' => true,
                        'success_url' => url('/payment-return?status=success'),
                        'cancel_url' => url('/payment-return?status=cancel'),
                        'payment_method_types' => ['gcash', 'paymaya', 'card'],
                        'line_items' => [
                            [
                                'currency' => 'PHP',
                                'amount' => (int) ($amount * 100), // In centavos (₱25.00 = 2500)
                                'description' => "Publication fee for {$title}",
                                'name' => "Campus Listing Fee",
                                'quantity' => 1,
                            ]
                        ],
                        'description' => "UniMart Campus Listing Fee",
                    ]
                ]
            ]);

        if ($response->successful()) {
            return $response->json('data');
        }

        \Log::error('PayMongo Create Checkout Error: ' . $response->body());
        return null;
    }

    /**
     * Retrieve status of a PayMongo Checkout Session
     */
    public function getCheckoutSession(string $sessionId): ?array
    {
        $response = Http::withBasicAuth($this->secretKey, '')
            ->get("{$this->baseUrl}/checkout_sessions/{$sessionId}");

        if ($response->successful()) {
            return $response->json('data');
        }

        \Log::error("PayMongo Get Checkout Error ({$sessionId}): " . $response->body());
        return null;
    }
}
