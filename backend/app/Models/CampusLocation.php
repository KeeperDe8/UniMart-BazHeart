<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class CampusLocation extends Model
{
    protected $fillable = ['name', 'landmark_notes'];

    public function listings()
    {
        return $this->hasMany(Listing::class, 'default_location_id');
    }

    public function schedules()
    {
        return $this->hasMany(SellingSchedule::class, 'location_id');
    }
}
