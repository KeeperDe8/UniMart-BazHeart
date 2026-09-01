-- ==========================================================
-- UniMart / BazHeart Campus Marketplace Database Schema
-- Database Name: unimart_db
-- Target: MySQL / MariaDB (XAMPP)
-- ==========================================================

CREATE DATABASE IF NOT EXISTS `unimart_db` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `unimart_db`;

SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS `saved_items`;
DROP TABLE IF EXISTS `notifications`;
DROP TABLE IF EXISTS `meetup_appointments`;
DROP TABLE IF EXISTS `messages`;
DROP TABLE IF EXISTS `conversation_participants`;
DROP TABLE IF EXISTS `conversations`;
DROP TABLE IF EXISTS `selling_schedules`;
DROP TABLE IF EXISTS `listing_payments`;
DROP TABLE IF EXISTS `listing_images`;
DROP TABLE IF EXISTS `listings`;
DROP TABLE IF EXISTS `campus_locations`;
DROP TABLE IF EXISTS `categories`;
DROP TABLE IF EXISTS `device_tokens`;
DROP TABLE IF EXISTS `personal_access_tokens`;
DROP TABLE IF EXISTS `users`;

SET FOREIGN_KEY_CHECKS = 1;

-- ==========================================================
-- 1. USERS & ROLES TABLE
-- ==========================================================
CREATE TABLE `users` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `student_number` VARCHAR(30) NULL UNIQUE,
    `name` VARCHAR(100) NOT NULL,
    `email` VARCHAR(150) NOT NULL UNIQUE,
    `password` VARCHAR(255) NOT NULL,
    `role` ENUM('buyer', 'seller', 'admin') NOT NULL DEFAULT 'buyer',
    `seller_shop_name` VARCHAR(100) NULL,
    `seller_bio` TEXT NULL,
    `avatar_url` VARCHAR(255) NULL,
    `preferred_meetup_area` VARCHAR(150) NULL DEFAULT 'Main Building – Ground Floor Lobby',
    `is_verified` TINYINT(1) NOT NULL DEFAULT 0,
    `is_seller_approved` TINYINT(1) NOT NULL DEFAULT 0,
    `remember_token` VARCHAR(100) NULL,
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==========================================================
-- 1.1 SELLERS TABLE (Approved Sellers & Shops)
-- ==========================================================
CREATE TABLE `sellers` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `user_id` BIGINT UNSIGNED NOT NULL UNIQUE,
    `shop_name` VARCHAR(100) NOT NULL,
    `bio` TEXT NULL,
    `status` ENUM('pending', 'approved', 'rejected') NOT NULL DEFAULT 'approved',
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==========================================================
-- 2. LARAVEL SANCTUM PERSONAL ACCESS TOKENS
-- ==========================================================
CREATE TABLE `personal_access_tokens` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `tokenable_type` VARCHAR(255) NOT NULL,
    `tokenable_id` BIGINT UNSIGNED NOT NULL,
    `name` VARCHAR(255) NOT NULL,
    `token` VARCHAR(64) NOT NULL UNIQUE,
    `abilities` TEXT NULL,
    `last_used_at` TIMESTAMP NULL,
    `expires_at` TIMESTAMP NULL,
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX `personal_access_tokens_tokenable_type_tokenable_id_index` (`tokenable_type`, `tokenable_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==========================================================
-- 3. DEVICE TOKENS (Push Notifications & Permissions)
-- ==========================================================
CREATE TABLE `device_tokens` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `user_id` BIGINT UNSIGNED NOT NULL,
    `device_token` TEXT NOT NULL,
    `device_type` ENUM('android', 'ios', 'windows') NOT NULL DEFAULT 'android',
    `is_active` TINYINT(1) NOT NULL DEFAULT 1,
    `last_used_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==========================================================
-- 4. CATEGORIES TABLE
-- ==========================================================
CREATE TABLE `categories` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(50) NOT NULL UNIQUE,
    `slug` VARCHAR(50) NOT NULL UNIQUE,
    `icon` VARCHAR(50) NULL,
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==========================================================
-- 5. CAMPUS LOCATIONS (Meetup Hotspots)
-- ==========================================================
CREATE TABLE `campus_locations` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `name` VARCHAR(100) NOT NULL UNIQUE,
    `landmark_notes` TEXT NULL,
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==========================================================
-- 6. LISTINGS TABLE
-- ==========================================================
CREATE TABLE `listings` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `seller_id` BIGINT UNSIGNED NOT NULL,
    `category_id` BIGINT UNSIGNED NOT NULL,
    `title` VARCHAR(150) NOT NULL,
    `description` TEXT NULL,
    `price` DECIMAL(10, 2) NOT NULL,
    `stock_quantity` INT NOT NULL DEFAULT 1,
    `item_condition` VARCHAR(50) NOT NULL DEFAULT 'Freshly Prepared / Baked',
    `default_location_id` BIGINT UNSIGNED NULL,
    `pickup_instructions` TEXT NULL,
    `status` ENUM('draft', 'pending_payment', 'active', 'sold_out', 'archived') NOT NULL DEFAULT 'pending_payment',
    `is_featured` TINYINT(1) NOT NULL DEFAULT 0,
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (`seller_id`) REFERENCES `users`(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`category_id`) REFERENCES `categories`(`id`),
    FOREIGN KEY (`default_location_id`) REFERENCES `campus_locations`(`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==========================================================
-- 7. LISTING IMAGES TABLE
-- ==========================================================
CREATE TABLE `listing_images` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `listing_id` BIGINT UNSIGNED NOT NULL,
    `image_path` VARCHAR(255) NOT NULL,
    `is_primary` TINYINT(1) NOT NULL DEFAULT 0,
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (`listing_id`) REFERENCES `listings`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==========================================================
-- 8. LISTING PAYMENTS (Pay-per-Listing GCash Fee Gateway)
-- ==========================================================
CREATE TABLE `listing_payments` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `listing_id` BIGINT UNSIGNED NOT NULL,
    `user_id` BIGINT UNSIGNED NOT NULL,
    `amount` DECIMAL(10, 2) NOT NULL DEFAULT 25.00,
    `payment_method` VARCHAR(30) NOT NULL DEFAULT 'GCash',
    `reference_number` VARCHAR(100) NOT NULL UNIQUE,
    `status` ENUM('pending', 'verified', 'rejected') NOT NULL DEFAULT 'verified',
    `verified_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (`listing_id`) REFERENCES `listings`(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==========================================================
-- 9. SELLING SCHEDULES (Campus Availability Slots)
-- ==========================================================
CREATE TABLE `selling_schedules` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `user_id` BIGINT UNSIGNED NOT NULL,
    `listing_id` BIGINT UNSIGNED NULL,
    `day_of_week` ENUM('Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday') NOT NULL,
    `time_window` VARCHAR(60) NOT NULL,
    `location_id` BIGINT UNSIGNED NULL,
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`listing_id`) REFERENCES `listings`(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`location_id`) REFERENCES `campus_locations`(`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==========================================================
-- 10. CONVERSATIONS & PARTICIPANTS
-- ==========================================================
CREATE TABLE `conversations` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `listing_id` BIGINT UNSIGNED NULL,
    `last_message_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (`listing_id`) REFERENCES `listings`(`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `conversation_participants` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `conversation_id` BIGINT UNSIGNED NOT NULL,
    `user_id` BIGINT UNSIGNED NOT NULL,
    `last_read_at` TIMESTAMP NULL,
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY `unique_conversation_user` (`conversation_id`, `user_id`),
    FOREIGN KEY (`conversation_id`) REFERENCES `conversations`(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==========================================================
-- 11. MESSAGES & MEETUP APPOINTMENTS
-- ==========================================================
CREATE TABLE `messages` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `conversation_id` BIGINT UNSIGNED NOT NULL,
    `sender_id` BIGINT UNSIGNED NOT NULL,
    `message_type` ENUM('text', 'image', 'meetup_card') NOT NULL DEFAULT 'text',
    `body` TEXT NOT NULL,
    `is_read` TINYINT(1) NOT NULL DEFAULT 0,
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (`conversation_id`) REFERENCES `conversations`(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`sender_id`) REFERENCES `users`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE `meetup_appointments` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `message_id` BIGINT UNSIGNED NOT NULL,
    `location_id` BIGINT UNSIGNED NULL,
    `scheduled_datetime` DATETIME NOT NULL,
    `status` ENUM('pending', 'confirmed', 'completed', 'cancelled') NOT NULL DEFAULT 'confirmed',
    `notes` VARCHAR(255) NULL DEFAULT 'Please be on time at the meetup hotspot.',
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (`message_id`) REFERENCES `messages`(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`location_id`) REFERENCES `campus_locations`(`id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==========================================================
-- 12. NOTIFICATIONS TABLE (In-App & Push Notifications)
-- ==========================================================
CREATE TABLE `notifications` (
    `id` CHAR(36) NOT NULL PRIMARY KEY,
    `user_id` BIGINT UNSIGNED NOT NULL,
    `type` VARCHAR(100) NOT NULL,
    `title` VARCHAR(150) NOT NULL,
    `body` TEXT NOT NULL,
    `data` JSON NULL,
    `read_at` TIMESTAMP NULL,
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==========================================================
-- 13. SAVED ITEMS (Wishlist)
-- ==========================================================
CREATE TABLE `saved_items` (
    `id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    `user_id` BIGINT UNSIGNED NOT NULL,
    `listing_id` BIGINT UNSIGNED NOT NULL,
    `created_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP,
    `updated_at` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY `unique_user_saved_listing` (`user_id`, `listing_id`),
    FOREIGN KEY (`user_id`) REFERENCES `users`(`id`) ON DELETE CASCADE,
    FOREIGN KEY (`listing_id`) REFERENCES `listings`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ==========================================================
-- SEED DATA (Ready-to-use demo data)
-- ==========================================================

-- Categories
INSERT INTO `categories` (`id`, `name`, `slug`, `icon`) VALUES
(1, 'Food & Drinks', 'food-drinks', '🍵'),
(2, 'Handmade', 'handmade', '🌸'),
(3, 'Clothes', 'clothes', '🧥'),
(4, 'Accessories', 'accessories', '📿'),
(5, 'School Supplies', 'school-supplies', '📚'),
(6, 'Other', 'other', '📦');

-- Campus Meetup Hotspots
INSERT INTO `campus_locations` (`id`, `name`, `landmark_notes`) VALUES
(1, 'Main Building – Ground Floor Lobby', 'Meet near the main building lobby benches.'),
(2, 'Student Activity Center (SAC)', 'Inside the student activity area / benches.'),
(3, 'Library Entrance', 'Outside 2nd floor library entrance hallway.'),
(4, 'Cafeteria', 'Central table area in student cafeteria.');

-- Users (Password is 'password' for all test users - bcrypt hashed)
INSERT INTO `users` (`id`, `student_number`, `name`, `email`, `password`, `role`, `seller_shop_name`, `seller_bio`, `avatar_url`, `preferred_meetup_area`, `is_verified`, `is_seller_approved`) VALUES
(1, '2023-100101', 'Maria Santos', 'maria.santos@gmail.com', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'seller', 'Maria Collections', 'Anime plushies, stationery & campus merch!', 'profile_sdada.jpg', 'Main Building – Ground Floor Lobby', 1, 1),
(2, '2022-104921', 'Kai dela Cruz', 'kai.delacruz@gmail.com', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'seller', 'Kai\'s Café & Matcha', 'Freshly whisked Uji matcha drinks & homemade fudgy baked treats on campus!', 'kai_avatar.jpg', 'Main Building – Ground Floor Lobby', 1, 1),
(3, '2022-205112', 'Samantha Reyes', 'samantha.reyes@gmail.com', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi', 'seller', 'Crochet by Sam', 'Handcrafted crochet flowers, bouquets, and cute accessories.', 'samantha_avatar.jpg', 'Student Activity Center (SAC)', 1, 1);

-- Approved Sellers Table Seed Data
INSERT INTO `sellers` (`id`, `user_id`, `shop_name`, `bio`, `status`) VALUES
(1, 1, 'Maria Collections', 'Anime plushies, stationery & campus merch!', 'approved'),
(2, 2, 'Kai\'s Café & Matcha', 'Freshly whisked Uji matcha drinks & homemade fudgy baked treats on campus!', 'approved'),
(3, 3, 'Crochet by Sam', 'Handcrafted crochet flowers, bouquets, and cute accessories.', 'approved');

-- Listings
INSERT INTO `listings` (`id`, `seller_id`, `category_id`, `title`, `description`, `price`, `stock_quantity`, `item_condition`, `default_location_id`, `pickup_instructions`, `status`, `is_featured`) VALUES
(1, 2, 1, 'Iced Strawberry Matcha Latte', 'Freshly whisked ceremonial Uji matcha layered over fresh milk and homemade strawberry puree. 16oz cup.', 95.00, 5, 'Freshly Prepared / Baked', 1, 'Meet near the lobby benches', 'active', 1),
(2, 3, 2, 'Handmade Crochet Daisy Bouquet', 'Beautiful pastel crochet daisy bouquet. Everlasting floral gift made with premium milk cotton yarn.', 160.00, 5, 'Brand New', 2, 'Meet at SAC entrance', 'active', 1),
(3, 2, 1, 'Fudgy Dark Chocolate Brownies', 'Rich, gooey, and fudgy triple chocolate brownies with chocolate chips.', 60.00, 8, 'Freshly Prepared / Baked', 1, 'Meet at Main Building Ground Floor', 'active', 0),
(4, 3, 4, 'Y2K Beaded Phone Charm Strap', 'Handmade trendy phone wrist strap with pastel glass beads and charms.', 80.00, 10, 'Brand New', 1, 'Meet at lobby or library entrance', 'active', 0),
(5, 1, 5, 'NU Lipa Die-Cut Stickers', 'Waterproof matte vinyl stickers with custom NU Lipa pride designs.', 35.00, 20, 'Brand New', 1, 'Meet near lobby', 'active', 0),
(6, 1, 3, 'Madoka Plush', 'Original Madoka Kaname plushie doll in pristine condition!', 700.00, 5, 'Good Condition', 1, 'Meet near lobby benches', 'active', 1),
(7, 1, 3, 'Homura Plush', 'Original Homura Akemi plushie doll in pristine condition!', 760.00, 5, 'Good Condition', 1, 'Meet near lobby benches', 'active', 1);

-- Listing Images
INSERT INTO `listing_images` (`id`, `listing_id`, `image_path`, `is_primary`) VALUES
(1, 1, 'matcha.jpg', 1),
(2, 2, 'crochet_bouquet.jpg', 1),
(3, 3, 'brownies.jpg', 1),
(4, 4, 'phone_charm.jpg', 1),
(5, 5, 'stickers.jpg', 1),
(6, 6, 'madoka.jpg', 1),
(7, 7, 'homura.jpg', 1);

-- Listing Payments (GCash Proof)
INSERT INTO `listing_payments` (`id`, `listing_id`, `user_id`, `amount`, `payment_method`, `reference_number`, `status`) VALUES
(1, 1, 2, 25.00, 'GCash', 'GCASH-LIPA-2026-343562', 'verified'),
(2, 2, 3, 25.00, 'GCash', 'GCASH-LIPA-2026-991204', 'verified'),
(3, 3, 2, 25.00, 'GCash', 'GCASH-LIPA-2026-778812', 'verified'),
(4, 4, 3, 25.00, 'GCash', 'GCASH-LIPA-2026-112233', 'verified'),
(5, 5, 1, 25.00, 'GCash', 'GCASH-LIPA-2026-445566', 'verified'),
(6, 6, 1, 25.00, 'GCash', 'GCASH-LIPA-2026-889900', 'verified'),
(7, 7, 1, 25.00, 'GCash', 'GCASH-LIPA-2026-889901', 'verified');

-- Selling Schedules
INSERT INTO `selling_schedules` (`id`, `user_id`, `listing_id`, `day_of_week`, `time_window`, `location_id`) VALUES
(1, 2, 1, 'Monday', '10:00 AM – 2:00 PM', 1),
(2, 2, 1, 'Wednesday', '11:00 AM – 3:00 PM', 1),
(3, 2, 1, 'Friday', '1:00 PM – 4:30 PM', 2),
(4, 3, 2, 'Tuesday', '12:00 PM – 4:00 PM', 2),
(5, 3, 2, 'Thursday', '12:00 PM – 4:00 PM', 2);

-- Sample Conversation between Maria Santos (Buyer) and Kai (Seller)
INSERT INTO `conversations` (`id`, `listing_id`, `last_message_at`) VALUES
(1, 1, CURRENT_TIMESTAMP);

INSERT INTO `conversation_participants` (`id`, `conversation_id`, `user_id`, `last_read_at`) VALUES
(1, 1, 1, CURRENT_TIMESTAMP),
(2, 1, 2, CURRENT_TIMESTAMP);

INSERT INTO `messages` (`id`, `conversation_id`, `sender_id`, `message_type`, `body`, `is_read`, `created_at`) VALUES
(1, 1, 1, 'text', 'Hi Kai! Is the Strawberry Cold Foam Matcha still available for pickup this afternoon?', 1, DATE_SUB(CURRENT_TIMESTAMP, INTERVAL 25 MINUTE)),
(2, 1, 2, 'text', 'Sure! I have 3 cups of Strawberry Matcha left today at the Main Lobby.', 1, DATE_SUB(CURRENT_TIMESTAMP, INTERVAL 15 MINUTE)),
(3, 1, 2, 'meetup_card', 'Campus Meetup Scheduled: Main Building – Ground Floor Lobby on Wednesday at 1:30 PM', 1, DATE_SUB(CURRENT_TIMESTAMP, INTERVAL 14 MINUTE));

INSERT INTO `meetup_appointments` (`id`, `message_id`, `location_id`, `scheduled_datetime`, `status`, `notes`) VALUES
(1, 3, 1, '2026-09-02 13:30:00', 'confirmed', 'Please be on time at the meetup hotspot.');

-- Notifications
INSERT INTO `notifications` (`id`, `user_id`, `type`, `title`, `body`, `read_at`) VALUES
('a1b2c3d4-e5f6-7890-abcd-ef1234567890', 2, 'ListingPublished', '🎉 Listing Published Successfully! 🚀', 'Iced Strawberry Matcha Latte is now live on UniMart campus feed!', NULL),
('b2c3d4e5-f6a7-8901-bcde-f12345678901', 2, 'PaymentConfirmed', '💳 Listing Payment Confirmed 🎉', 'Your listing fee for Iced Matcha Latte was confirmed via GCash.', NULL),
('c3d4e5f6-a7b8-9012-cdef-123456789012', 2, 'NewMessage', '💬 New Product Inquiry', 'Maria Santos sent an inquiry about Strawberry Cold Foam Matcha.', NULL);

-- Saved Items (Wishlist)
INSERT INTO `saved_items` (`id`, `user_id`, `listing_id`) VALUES
(1, 1, 1),
(2, 1, 2);
