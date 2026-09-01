<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Factories\HasFactory;
use Illuminate\Foundation\Auth\User as Authenticatable;
use Illuminate\Notifications\Notifiable;
use Laravel\Sanctum\HasApiTokens;

class User extends Authenticatable
{
    use HasApiTokens, HasFactory, Notifiable;

    protected $fillable = [
        'student_number',
        'name',
        'email',
        'password',
        'role',
        'seller_shop_name',
        'seller_bio',
        'avatar_url',
        'preferred_meetup_area',
        'is_verified',
        'is_seller_approved',
    ];

    protected $hidden = [
        'password',
        'remember_token',
    ];

    protected function casts(): array
    {
        return [
            'password' => 'hashed',
            'is_verified' => 'boolean',
            'is_seller_approved' => 'boolean',
        ];
    }

    public function seller()
    {
        return $this->hasOne(Seller::class, 'user_id');
    }

    public function listings()
    {
        return $this->hasMany(Listing::class, 'seller_id');
    }

    public function payments()
    {
        return $this->hasMany(ListingPayment::class);
    }

    public function sellingSchedules()
    {
        return $this->hasMany(SellingSchedule::class);
    }

    public function deviceTokens()
    {
        return $this->hasMany(DeviceToken::class);
    }

    public function savedItems()
    {
        return $this->hasMany(SavedItem::class);
    }

    public function conversations()
    {
        return $this->belongsToMany(Conversation::class, 'conversation_participants')
                    ->withPivot('last_read_at')
                    ->withTimestamps();
    }

    public function notifications()
    {
        return $this->hasMany(Notification::class);
    }
}
