<?php

use App\Http\Controllers\Api\AuthController;
use App\Http\Controllers\Api\ChatController;
use App\Http\Controllers\Api\CommonController;
use App\Http\Controllers\Api\ListingController;
use App\Http\Controllers\Api\NotificationController;
use App\Http\Controllers\Api\PaymentController;
use App\Http\Controllers\Api\SavedItemController;
use App\Http\Controllers\Api\ScheduleController;
use Illuminate\Support\Facades\Route;

// ==========================================
// 1. PUBLIC ROUTES & CORE MARKETPLACE
// ==========================================
Route::post('/otp/send', [AuthController::class, 'sendOtp']);
Route::post('/otp/verify', [AuthController::class, 'verifyOtp']);

Route::post('/register', [AuthController::class, 'register']);
Route::post('/login', [AuthController::class, 'login']);
Route::post('/seller/apply', [AuthController::class, 'applySeller']);

Route::get('/categories', [CommonController::class, 'categories']);
Route::get('/locations', [CommonController::class, 'locations']);
Route::get('/sellers/{id}/shop', [CommonController::class, 'publicSellerShop']);

Route::get('/listings', [ListingController::class, 'index']);
Route::get('/listings/{id}', [ListingController::class, 'show']);

// Listing Creation & PayMongo Payment
Route::post('/listings', [ListingController::class, 'store']);
Route::post('/listings/{id}/paymongo-link', [PaymentController::class, 'createPaymongoLink']);
Route::post('/listings/{id}/pay-fee', [PaymentController::class, 'payFee']);
Route::match(['get', 'post'], '/listings/{id}/check-payment', [PaymentController::class, 'checkPaymentStatus']);
Route::post('/webhooks/paymongo', [PaymentController::class, 'handleWebhook']);
Route::get('/seller/my-listings', [ListingController::class, 'mySellerListings']);

// Schedules
Route::get('/schedules', [ScheduleController::class, 'index']);
Route::post('/schedules', [ScheduleController::class, 'store']);
Route::delete('/schedules/{id}', [ScheduleController::class, 'destroy']);

// Messaging & Meetups
Route::get('/conversations', [ChatController::class, 'conversations']);
Route::get('/conversations/{id}/messages', [ChatController::class, 'messages']);
Route::post('/messages', [ChatController::class, 'sendMessage']);

// Notifications
Route::get('/notifications', [NotificationController::class, 'index']);
Route::post('/notifications/mark-all-read', [NotificationController::class, 'markAllAsRead']);
Route::post('/device-tokens', [NotificationController::class, 'registerDeviceToken']);

// Saved Items / Wishlist
Route::get('/saved-items', [SavedItemController::class, 'index']);
Route::post('/saved-items/{listingId}/toggle', [SavedItemController::class, 'toggle']);

// ==========================================
// 2. USER INFO (token-aware but won't crash without it)
Route::get('/me', [AuthController::class, 'me']);

// 3. AUTHENTICATED USER ROUTES
Route::middleware('auth:sanctum')->group(function () {
    Route::put('/profile', [AuthController::class, 'updateProfile']);
    Route::post('/logout', [AuthController::class, 'logout']);
});
