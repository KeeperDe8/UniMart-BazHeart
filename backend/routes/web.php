<?php

use Illuminate\Support\Facades\Route;

Route::get('/', function () {
    return view('welcome');
});

Route::get('/payment-return', function () {
    return response('<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Payment Successful - UniMart</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
            background: #F7F9FC;
            display: flex;
            align-items: center;
            justify-content: center;
            min-height: 100vh;
            margin: 0;
            padding: 20px;
            box-sizing: border-box;
        }
        .card {
            background: #FFFFFF;
            border-radius: 24px;
            padding: 36px 24px;
            max-width: 400px;
            width: 100%;
            text-align: center;
            box-shadow: 0 10px 30px rgba(36, 86, 216, 0.08);
            border: 1px solid #DFE6EF;
        }
        .icon {
            width: 72px;
            height: 72px;
            background: #CFF7DF;
            color: #16A66A;
            border-radius: 50%;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 38px;
            margin: 0 auto 20px;
            font-weight: bold;
        }
        h1 {
            color: #172033;
            font-size: 22px;
            margin: 0 0 8px;
            font-weight: 700;
        }
        p {
            color: #8A9AAF;
            font-size: 14px;
            line-height: 1.5;
            margin: 0 0 24px;
        }
        .timer-box {
            background: #F1F5FA;
            padding: 12px;
            border-radius: 12px;
            font-size: 13px;
            color: #193F9D;
            font-weight: 600;
            margin-bottom: 24px;
        }
        .btn {
            display: block;
            background: #2456D8;
            color: #FFFFFF;
            text-decoration: none;
            padding: 15px 24px;
            border-radius: 16px;
            font-weight: 700;
            font-size: 15px;
        }
    </style>
</head>
<body>
    <div class="card">
        <div class="icon">✓</div>
        <h1>Payment Completed! 🎉</h1>
        <p>Your listing fee of ₱25.00 has been verified. Your product is ready to go live on UniMart!</p>
        
        <div class="timer-box">
            Returning to UniMart app in <span id="count">3</span>s...
        </div>

        <a href="unimart://payment-success" class="btn">📲 Return to UniMart App</a>
    </div>

    <script>
        let seconds = 3;
        const countEl = document.getElementById("count");
        const returnUrl = "unimart://payment-success";

        const interval = setInterval(() => {
            seconds--;
            if (countEl) countEl.innerText = seconds;
            if (seconds <= 0) {
                clearInterval(interval);
                window.location.href = returnUrl;
            }
        }, 1000);

        setTimeout(() => {
            window.location.href = returnUrl;
        }, 3000);
    </script>
</body>
</html>', 200, ['Content-Type' => 'text/html']);
});
