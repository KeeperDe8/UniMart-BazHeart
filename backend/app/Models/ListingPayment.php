<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class ListingPayment extends Model
{
    protected $fillable = [
        'listing_id',
        'user_id',
        'amount',
        'payment_method',
        'reference_number',
        'status',
        'verified_at',
    ];

    protected $casts = [
        'amount' => 'decimal:2',
        'verified_at' => 'datetime',
    ];

    public function listing()
    {
        return $this->belongsTo(Listing::class);
    }

    public function user()
    {
        return $this->belongsTo(User::class);
    }
}
