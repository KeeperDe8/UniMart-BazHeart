<?php

namespace App\Services;

use App\Models\User;
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
            Mail::html("
                <div style='font-family: Arial, sans-serif; max-width: 500px; margin: auto; padding: 24px; background: #0B111E; color: #FFFFFF; border-radius: 16px;'>
                    <h2 style='color: #2563EB; margin-bottom: 8px;'>BazHeart NU Lipa</h2>
                    <p style='color: #94A3B8; font-size: 14px;'>Find. Share. Connect.</p>
                    <hr style='border: none; border-top: 1px solid #1E293B; margin: 16px 0;'/>
                    <p style='font-size: 15px;'>Here is your 6-digit verification code to activate your campus marketplace account:</p>
                    <div style='background: #131D31; padding: 18px; text-align: center; border-radius: 12px; margin: 20px 0;'>
                        <span style='font-size: 34px; font-weight: bold; letter-spacing: 6px; color: #38BDF8;'>{$code}</span>
                    </div>
                    <p style='color: #94A3B8; font-size: 13px;'>This code expires in 10 minutes. If you did not request this code, you can safely ignore this email.</p>
                    <p style='color: #64748B; font-size: 11px; margin-top: 24px;'>— BazHeart NU Lipa Campus Marketplace Team</p>
                </div>
            ", function ($message) use ($email) {
                $message->to($email)
                        ->subject('BazHeart — Your 6-Digit Verification Code');
            });
        } catch (\Exception $e) {
            \Log::error("Failed to send OTP to {$email}: " . $e->getMessage());
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

        // Update database user verified status
        User::where('email', $email)->update([
            'is_verified' => true,
            'email_verified_at' => now(),
        ]);

        return true;
    }
}

