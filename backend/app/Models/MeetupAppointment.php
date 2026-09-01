<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MeetupAppointment extends Model
{
    protected $fillable = [
        'message_id',
        'location_id',
        'scheduled_datetime',
        'status',
        'notes',
    ];

    protected $casts = [
        'scheduled_datetime' => 'datetime',
    ];

    public function message()
    {
        return $this->belongsTo(Message::class);
    }

    public function location()
    {
        return $this->belongsTo(CampusLocation::class, 'location_id');
    }
}
