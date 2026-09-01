<?php

require __DIR__ . '/../vendor/autoload.php';
$app = require_once __DIR__ . '/../bootstrap/app.php';
$app->make(\Illuminate\Contracts\Console\Kernel::class)->bootstrap();

use Illuminate\Support\Facades\DB;
use Illuminate\Support\Facades\Schema;
use Illuminate\Database\Schema\Blueprint;

echo "=== CREATING SELLERS TABLE & RUNNING REPAIR ===" . PHP_EOL;

// 1. Create Sellers Table if not exists
if (!Schema::hasTable('sellers')) {
    Schema::create('sellers', function (Blueprint $table) {
        $table->id();
        $table->unsignedBigInteger('user_id')->unique();
        $table->string('shop_name', 100);
        $table->text('bio')->nullable();
        $table->enum('status', ['pending', 'approved', 'rejected'])->default('approved');
        $table->timestamps();

        $table->foreign('user_id')->references('id')->on('users')->onDelete('cascade');
    });
    echo "Table 'sellers' created successfully." . PHP_EOL;
} else {
    echo "Table 'sellers' already exists." . PHP_EOL;
}

// 2. Populate Sellers Table with approved sellers
DB::table('sellers')->insertOrIgnore([
    [
        'id' => 1,
        'user_id' => 1,
        'shop_name' => 'Maria Collections',
        'bio' => 'Anime plushies, stationery & campus merch!',
        'status' => 'approved',
        'created_at' => now(),
        'updated_at' => now(),
    ],
    [
        'id' => 2,
        'user_id' => 2,
        'shop_name' => "Kai's Café & Matcha",
        'bio' => 'Freshly whisked Uji matcha drinks & homemade fudgy baked treats on campus!',
        'status' => 'approved',
        'created_at' => now(),
        'updated_at' => now(),
    ],
    [
        'id' => 3,
        'user_id' => 3,
        'shop_name' => 'Crochet by Sam',
        'bio' => 'Handcrafted crochet flowers, bouquets, and cute accessories.',
        'status' => 'approved',
        'created_at' => now(),
        'updated_at' => now(),
    ],
]);

// 3. Ensure user 1 has seller status and correct shop name
DB::table('users')->where('id', 1)->update([
    'role' => 'seller',
    'seller_shop_name' => 'Maria Collections',
    'seller_bio' => 'Anime plushies, stationery & campus merch!',
    'is_seller_approved' => 1,
    'is_verified' => 1,
]);

// 4. Ensure all listings are active and connected
DB::table('listings')->where('status', 'pending_payment')->update(['status' => 'active']);

echo "=== MIGRATION COMPLETE! ===" . PHP_EOL;
$sellers = DB::table('sellers')->get();
foreach ($sellers as $s) {
    echo "Seller ID: {$s->id} | User ID: {$s->user_id} | Shop: {$s->shop_name} | Status: {$s->status}" . PHP_EOL;
}
