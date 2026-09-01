<?php

namespace App\Http\Controllers\Api;

use App\Http\Controllers\Controller;
use App\Services\AuthService;
use App\Services\OtpService;
use Illuminate\Http\JsonResponse;
use Illuminate\Http\Request;

class AuthController extends Controller
{
    public function __construct(
        protected AuthService $authService,
        protected OtpService $otpService
    ) {}

    public function sendOtp(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'email' => 'required|email',
        ]);

        $this->otpService->sendOtp($validated['email']);

        return response()->json([
            'message' => 'Verification code sent to your email',
        ]);
    }

    public function verifyOtp(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'email' => 'required|email',
            'code' => 'required|string|size:6',
        ]);

        $this->otpService->verifyOtp($validated['email'], $validated['code']);

        return response()->json([
            'message' => 'Email verified successfully',
        ]);
    }

    public function register(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'name' => 'required|string|max:100',
            'email' => 'required|email|max:150|unique:users,email',
            'password' => 'required|string|min:6',
            'student_number' => 'nullable|string|max:30|unique:users,student_number',
            'role' => 'nullable|in:buyer,seller',
            'seller_shop_name' => 'nullable|string|max:100',
            'seller_bio' => 'nullable|string',
            'preferred_meetup_area' => 'nullable|string|max:150',
        ]);

        $result = $this->authService->register($validated);

        return response()->json([
            'message' => 'Registration successful',
            'user' => $result['user'],
            'token' => $result['token'],
        ], 201);
    }

    public function login(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'email' => 'required|email',
            'password' => 'required|string',
        ]);

        $result = $this->authService->login($validated['email'], $validated['password']);

        return response()->json([
            'message' => 'Login successful',
            'user' => $result['user'],
            'token' => $result['token'],
        ]);
    }

    public function me(Request $request): JsonResponse
    {
        $user = $request->user() ?? $request->user('sanctum');
        
        // Fallback: lookup by email if token can't resolve
        if (!$user && $request->query('email')) {
            $user = \App\Models\User::where('email', $request->query('email'))->first();
        }
        
        return response()->json([
            'user' => $user,
        ]);
    }

    public function updateProfile(Request $request): JsonResponse
    {
        $validated = $request->validate([
            'name' => 'nullable|string|max:100',
            'seller_shop_name' => 'nullable|string|max:100',
            'seller_bio' => 'nullable|string',
            'preferred_meetup_area' => 'nullable|string|max:150',
            'avatar_url' => 'nullable|string|max:255',
        ]);

        $user = $this->authService->updateProfile($request->user(), $validated);

        return response()->json([
            'message' => 'Profile updated successfully',
            'user' => $user,
        ]);
    }

    public function applySeller(Request $request): JsonResponse
    {
        $user = $request->user() ?? $request->user('sanctum') ?? \App\Models\User::where('role', 'seller')->first() ?? \App\Models\User::first();
        if ($user) {
            $user->update([
                'role' => 'seller',
                'is_seller_approved' => 1,
                'is_verified' => 1,
                'seller_shop_name' => $request->input('shop_name', $user->seller_shop_name ?? ($user->name . "'s Campus Shop")),
                'seller_bio' => $request->input('bio', $user->seller_bio ?? 'Verified Student Seller on NU Lipa Campus'),
            ]);
        }

        return response()->json([
            'message' => 'Seller application auto-approved successfully!',
            'user' => $user,
        ]);
    }

    public function logout(Request $request): JsonResponse
    {
        if ($request->user()) {
            $this->authService->logout($request->user());
        }

        return response()->json([
            'message' => 'Logged out successfully',
        ]);
    }
}
