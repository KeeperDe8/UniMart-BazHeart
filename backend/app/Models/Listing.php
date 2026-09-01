<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class Listing extends Model
{
    protected $fillable = [
        'seller_id',
        'category_id',
        'title',
        'description',
        'price',
        'stock_quantity',
        'item_condition',
        'default_location_id',
        'pickup_instructions',
        'status',
        'is_featured',
    ];

    protected $casts = [
        'price' => 'float',
        'stock_quantity' => 'integer',
        'is_featured' => 'boolean',
    ];

    public function seller()
    {
        return $this->belongsTo(User::class, 'seller_id');
    }

    public function category()
    {
        return $this->belongsTo(Category::class);
    }

    public function location()
    {
        return $this->belongsTo(CampusLocation::class, 'default_location_id');
    }

    public function images()
    {
        return $this->hasMany(ListingImage::class);
    }

    public function primaryImage()
    {
        return $this->hasOne(ListingImage::class)->where('is_primary', true);
    }

    public function payments()
    {
        return $this->hasMany(ListingPayment::class);
    }

    public function schedules()
    {
        return $this->hasMany(SellingSchedule::class);
    }

    public function savedByUsers()
    {
        return $this->hasMany(SavedItem::class);
    }
}
