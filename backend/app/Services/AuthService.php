<?php

namespace App\Services;

use App\Models\User;
use Illuminate\Support\Facades\Hash;
use Illuminate\Validation\ValidationException;

class AuthService
{
    public function register(array $data): array
    {
        $role = $data['role'] ?? 'buyer';

        $user = User::create([
            'name' => $data['name'],
            'email' => $data['email'],
            'password' => Hash::make($data['password']),
            'student_number' => $data['student_number'] ?? null,
            'role' => $role,
            'seller_shop_name' => $role === 'seller' ? ($data['seller_shop_name'] ?? null) : null,
            'seller_bio' => $role === 'seller' ? ($data['seller_bio'] ?? null) : null,
            'preferred_meetup_area' => $data['preferred_meetup_area'] ?? 'Main Building – Ground Floor Lobby',
            'is_verified' => false,
        ]);

        $token = $user->createToken('auth_token')->plainTextToken;

        return [
            'user' => $user,
            'token' => $token,
        ];
    }

    public function login(string $email, string $password): array
    {
        $user = User::where('email', $email)->first();

        if (!$user || !Hash::check($password, $user->password)) {
            throw ValidationException::withMessages([
                'email' => ['Invalid email or password.'],
            ]);
        }

        $token = $user->createToken('auth_token')->plainTextToken;

        return [
            'user' => $user,
            'token' => $token,
        ];
    }

    public function logout(User $user): void
    {
        $user->currentAccessToken()?->delete();
    }

    public function updateProfile(User $user, array $data): User
    {
        $user->update(array_filter([
            'name' => $data['name'] ?? null,
            'seller_shop_name' => $data['seller_shop_name'] ?? null,
            'seller_bio' => $data['seller_bio'] ?? null,
            'preferred_meetup_area' => $data['preferred_meetup_area'] ?? null,
            'avatar_url' => $data['avatar_url'] ?? null,
        ]));

        return $user->fresh();
    }
}
