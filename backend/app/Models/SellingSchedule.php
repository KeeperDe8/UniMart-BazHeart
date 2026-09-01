<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class SellingSchedule extends Model
{
    protected $fillable = [
        'user_id',
        'listing_id',
        'day_of_week',
        'time_window',
        'location_id',
    ];

    public function user()
    {
        return $this->belongsTo(User::class);
    }

    public function listing()
    {
        return $this->belongsTo(Listing::class);
    }

    public function location()
    {
        return $this->belongsTo(CampusLocation::class, 'location_id');
    }
}
