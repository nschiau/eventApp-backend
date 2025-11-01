-- Initialize EventHorizon Database
-- This script will be run when the PostgreSQL container starts for the first time

-- Create uuid extension for generating UUIDs
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Users table
CREATE TABLE IF NOT EXISTS users (
    id VARCHAR PRIMARY KEY DEFAULT gen_random_uuid(),
    username TEXT NOT NULL UNIQUE,
    password TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Events table
CREATE TABLE IF NOT EXISTS events (
    id VARCHAR PRIMARY KEY DEFAULT gen_random_uuid(),
    title TEXT NOT NULL,
    description TEXT NOT NULL,
    category TEXT NOT NULL,
    date TIMESTAMP NOT NULL,
    location TEXT NOT NULL,
    image_url TEXT,
    created_by_id VARCHAR REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Insert sample data
-- INSERT INTO events (title, description, category, date, location, image_url) VALUES 
-- ('Rockstadt Extreme Fest 12', 'Romania''s premier extreme metal festival featuring international and local metal bands. A 5-day extreme metal experience with camping facilities and multiple stages.', 'Concerts', '2026-07-27 13:00:00', 'Ghimbav, Brașov County, Romania', '/attached_assets/stock_images/rockstadt-extreme-fest.jpg'),
-- ('Metallica - M72 World Tour', 'Metallica brings their M72 World Tour to Bucharest with special guests Gojira and Knocked Loose. An unforgettable heavy metal experience at Arena Națională.', 'Concerts', '2026-05-13 19:00:00', 'Arena Națională, Bucharest, Romania', '/attached_assets/stock_images/metallica-m72-tour.jpg'),
-- ('QFest - Ziua III - Robin and the Backstabbers', 'Day 3 of QFest featuring Robin and the Backstabbers, The Kryptonite Sparks, and Nuante. An evening of indie alternative rock and poetic lyrics.', 'Concerts', '2025-10-01 18:00:00', 'Quantic Club, Soseaua Grozăvești 82, Bucharest, Romania', '/attached_assets/stock_images/qfest-day-3.jpg');

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_events_date ON events(date);
CREATE INDEX IF NOT EXISTS idx_events_category ON events(category);
CREATE INDEX IF NOT EXISTS idx_events_created_by_id ON events(created_by_id);
