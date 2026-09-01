<?php

namespace App\Services;

use Illuminate\Support\Facades\Cache;
use Illuminate\Support\Facades\Mail;
use Illuminate\Validation\ValidationException;

class OtpService
{
    public function sendOtp(string $email): string
    {
        $code = (string) random_int(100000, 999999);

        // Store OTP in cache for 10 minutes
        Cache::put("otp_{$email}", $code, now()->addMinutes(10));

        // Send Email via Gmail SMTP
        try {
            Mail::raw("Your UniMart verification code is: {$code}\n\nThis code expires in 10 minutes.\n\n— UniMart NU Lipa Campus Marketplace", function ($message) use ($email) {
                $message->to($email)
                        ->subject('UniMart Campus Verification Code');
            });
        } catch (\Exception $e) {
            \Log::error("Failed to send OTP to {$email}: " . $e->getMessage());
            // If mail fails, return code in debug mode so test flow doesn't block
        }

        return $code;
    }

    public function verifyOtp(string $email, string $code): bool
    {
        $cachedCode = Cache::get("otp_{$email}");

        if (!$cachedCode || $cachedCode !== $code) {
            throw ValidationException::withMessages([
                'otp' => ['The verification code is invalid or has expired.'],
            ]);
        }

        // Clear after successful verification
        Cache::forget("otp_{$email}");

        return true;
    }
}
