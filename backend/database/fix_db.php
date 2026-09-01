<?php

require __DIR__ . '/../vendor/autoload.php';
$app = require_once __DIR__ . '/../bootstrap/app.php';
$app->make(\Illuminate\Contracts\Console\Kernel::class)->bootstrap();

use App\Models\User;
use App\Models\Listing;
use App\Models\ListingImage;
use Illuminate\Support\Facades\DB;

echo "=== FIXING UNIMART DATABASE ===" . PHP_EOL;

// 1. Update Maria Santos (ID 1) as verified Seller
DB::table('users')->where('id', 1)->update([
    'role' => 'seller',
    'seller_shop_name' => 'Maria Collections',
    'seller_bio' => 'Anime plushies, stationeries & campus merch!',
    'is_seller_approved' => 1,
    'is_verified' => 1
]);
echo "User 1 updated to Seller." . PHP_EOL;

// 2. Activate all pending listings so Madoka and Homura are live
DB::table('listings')->where('status', 'pending_payment')->update([
    'status' => 'active'
]);
echo "All pending listings activated to 'active'." . PHP_EOL;

// 3. Remove duplicate spam listings while keeping Madoka (15), Homura (18), etc.
$deleteIds = [6, 7, 8, 9, 10, 11, 12, 13, 14, 16, 17];
DB::table('listings')->whereIn('id', $deleteIds)->delete();
echo "Duplicate spam listings removed." . PHP_EOL;

// 4. Ensure Madoka Plush and Homura Plush have proper primary images and categories
$madoka = Listing::where('title', 'like', '%Madoka%')->first();
if ($madoka) {
    $madoka->update([
        'seller_id' => 1,
        'category_id' => 3, // Clothes & Merch
        'status' => 'active',
        'price' => 700.00,
        'description' => 'Original Madoka Kaname plushie doll in pristine condition!'
    ]);
    ListingImage::firstOrCreate(
        ['listing_id' => $madoka->id],
        ['image_path' => 'madoka.jpg', 'is_primary' => true]
    );
}

$homura = Listing::where('title', 'like', '%Homura%')->first();
if ($homura) {
    $homura->update([
        'seller_id' => 1,
        'category_id' => 3, // Clothes & Merch
        'status' => 'active',
        'price' => 760.00,
        'description' => 'Original Homura Akemi plushie doll in pristine condition!'
    ]);
    ListingImage::firstOrCreate(
        ['listing_id' => $homura->id],
        ['image_path' => 'homura.jpg', 'is_primary' => true]
    );
}

// 5. Ensure Maria has her listings linked
echo PHP_EOL . "=== CURRENT LISTINGS IN DATABASE ===" . PHP_EOL;
$listings = Listing::with(['seller', 'category', 'primaryImage'])->get();
foreach ($listings as $l) {
    echo "ID: {$l->id} | Seller: {$l->seller?->name} (ID: {$l->seller_id}) | Title: {$l->title} | Price: ₱{$l->price} | Status: {$l->status} | Category: {$l->category?->name}" . PHP_EOL;
}
