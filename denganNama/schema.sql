-- =======================================================================
-- SKEMA BASIS DATA REVISI: EDOM BERBASIS LOGIKA FUZZY (EDOM-FUZZY)
-- Proyek PBL Kecerdasan Buatan - Jurusan TIK Politeknik Negeri Jakarta
-- Engine: MySQL / MariaDB (Strict Constraints, Soft Delete, True Anonymity)
-- =======================================================================

SET FOREIGN_KEY_CHECKS = 0;
DROP TABLE IF EXISTS hasil_fuzzy_dosen;
DROP TABLE IF EXISTS jawaban;
DROP TABLE IF EXISTS pengisian_kuesioner;
DROP TABLE IF EXISTS pertanyaan;
DROP TABLE IF EXISTS pengampu;
DROP TABLE IF EXISTS periode_edom;
DROP TABLE IF EXISTS matkul;
DROP TABLE IF EXISTS users;
DROP TABLE IF EXISTS kelas;
SET FOREIGN_KEY_CHECKS = 1;

-- -----------------------------------------------------------------------
-- 1. Tabel Master Kelas (Menyimpan Identitas Permanen Rombel)
-- -----------------------------------------------------------------------
CREATE TABLE kelas (
    id INT AUTO_INCREMENT PRIMARY KEY,
    prodi VARCHAR(100) NOT NULL DEFAULT 'Sarjana Terapan Teknik Informatika',
    angkatan INT NOT NULL,                  -- Contoh: 2025
    rombel CHAR(2) NOT NULL,               -- Contoh: 'A', 'B', 'C'
    aktif BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_kelas UNIQUE (prodi, angkatan, rombel)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -----------------------------------------------------------------------
-- 2. Tabel Master Pengguna (Mahasiswa, Dosen, Admin)
-- -----------------------------------------------------------------------
CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,  -- NIM (Mhs), NIP (Dosen), Username (Admin)
    password VARCHAR(255) NOT NULL,        -- Hash BCrypt
    nama VARCHAR(100) NOT NULL,
    role ENUM('mahasiswa', 'dosen', 'admin') NOT NULL,
    id_kelas INT NULL,                      -- Khusus Mahasiswa, NULL untuk Dosen/Admin
    aktif BOOLEAN NOT NULL DEFAULT TRUE,   -- Soft Delete
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_users_kelas FOREIGN KEY (id_kelas) REFERENCES kelas(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -----------------------------------------------------------------------
-- 3. Tabel Master Mata Kuliah
-- -----------------------------------------------------------------------
CREATE TABLE matkul (
    id INT AUTO_INCREMENT PRIMARY KEY,
    kode_matkul VARCHAR(20) NOT NULL UNIQUE,
    nama_matkul VARCHAR(100) NOT NULL,
    sks INT NOT NULL DEFAULT 2,
    aktif BOOLEAN NOT NULL DEFAULT TRUE,   -- Soft Delete
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -----------------------------------------------------------------------
-- 4. Tabel Manajemen Periode EDOM (Jadwal Semester Aktif)
-- -----------------------------------------------------------------------
CREATE TABLE periode_edom (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nama_periode VARCHAR(50) NOT NULL,    -- Contoh: '2025/2026 Genap'
    tahun_ajaran INT NOT NULL,             -- Contoh: 2025
    semester ENUM('Ganjil', 'Genap') NOT NULL,
    tanggal_mulai DATE NOT NULL,
    tanggal_selesai DATE NOT NULL,
    status_aktif BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_periode UNIQUE (tahun_ajaran, semester)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -----------------------------------------------------------------------
-- 5. Tabel Penugasan Dosen Pengampu (Jadwal Pengajaran Dosen per Kelas)
-- -----------------------------------------------------------------------
CREATE TABLE pengampu (
    id INT AUTO_INCREMENT PRIMARY KEY,
    id_dosen INT NOT NULL,
    id_matkul INT NOT NULL,
    id_kelas INT NOT NULL,
    id_periode INT NOT NULL,
    aktif BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_pengampu UNIQUE (id_dosen, id_matkul, id_kelas, id_periode),
    CONSTRAINT fk_pengampu_dosen FOREIGN KEY (id_dosen) REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_pengampu_matkul FOREIGN KEY (id_matkul) REFERENCES matkul(id) ON DELETE RESTRICT,
    CONSTRAINT fk_pengampu_kelas FOREIGN KEY (id_kelas) REFERENCES kelas(id) ON DELETE RESTRICT,
    CONSTRAINT fk_pengampu_periode FOREIGN KEY (id_periode) REFERENCES periode_edom(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -----------------------------------------------------------------------
-- 6. Tabel Master Instrumen Pertanyaan EDOM (27 Butir Instrumen PNJ)
-- -----------------------------------------------------------------------
CREATE TABLE pertanyaan (
    id INT AUTO_INCREMENT PRIMARY KEY,
    kategori ENUM('pedagogik', 'profesional', 'kepribadian', 'sosial') NOT NULL,
    pertanyaan TEXT NOT NULL,
    aktif BOOLEAN NOT NULL DEFAULT TRUE,   -- Soft Delete (Soal tidak boleh di-hard delete)
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -----------------------------------------------------------------------
-- 7. Tabel Status Pengisian Kuesioner (Verifikasi Tanpa Melacak Nilai)
-- -----------------------------------------------------------------------
CREATE TABLE pengisian_kuesioner (
    id INT AUTO_INCREMENT PRIMARY KEY,
    id_pengampu INT NOT NULL,
    id_mahasiswa INT NOT NULL,
    tanggal_isi DATE NOT NULL,             -- Diambil dari CURDATE() server
    CONSTRAINT uq_pengisian UNIQUE (id_pengampu, id_mahasiswa),
    CONSTRAINT fk_pengisian_pengampu FOREIGN KEY (id_pengampu) REFERENCES pengampu(id) ON DELETE RESTRICT,
    CONSTRAINT fk_pengisian_mahasiswa FOREIGN KEY (id_mahasiswa) REFERENCES users(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -----------------------------------------------------------------------
-- 8. Tabel Transaksional Jawaban Kuesioner (TRUE ANONYMITY)
-- Tidak menyimpan id_mahasiswa & tidak ada timestamp waktu agar nir-korelasi
-- -----------------------------------------------------------------------
CREATE TABLE jawaban (
    id INT AUTO_INCREMENT PRIMARY KEY,
    id_pengampu INT NOT NULL,
    id_pertanyaan INT NOT NULL,
    nilai TINYINT NOT NULL,
    CONSTRAINT chk_jawaban_nilai CHECK (nilai BETWEEN 1 AND 5),
    CONSTRAINT fk_jawaban_pengampu FOREIGN KEY (id_pengampu) REFERENCES pengampu(id) ON DELETE RESTRICT,
    CONSTRAINT fk_jawaban_pertanyaan FOREIGN KEY (id_pertanyaan) REFERENCES pertanyaan(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- -----------------------------------------------------------------------
-- 9. Tabel Rekap Hasil Evaluasi Logika Fuzzy Dosen
-- -----------------------------------------------------------------------
CREATE TABLE hasil_fuzzy_dosen (
    id INT AUTO_INCREMENT PRIMARY KEY,
    id_dosen INT NOT NULL,
    id_periode INT NOT NULL,
    skor_pedagogik FLOAT NOT NULL,
    skor_profesional FLOAT NOT NULL,
    skor_kepribadian FLOAT NOT NULL,
    skor_sosial FLOAT NOT NULL,
    skor_akhir_fuzzy FLOAT NOT NULL,
    predikat VARCHAR(50) NOT NULL,
    rekomendasi TEXT NOT NULL,
    calculated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT uq_hasil_dosen_periode UNIQUE (id_dosen, id_periode),
    CONSTRAINT fk_fuzzy_dosen FOREIGN KEY (id_dosen) REFERENCES users(id) ON DELETE RESTRICT,
    CONSTRAINT fk_fuzzy_periode FOREIGN KEY (id_periode) REFERENCES periode_edom(id) ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- =======================================================================
-- SEED DATA AWAL (BCrypt Hash: password default 'admin123', 'dosen123', 'mhs123')
-- =======================================================================

-- Master Rombel Kelas
INSERT INTO kelas (prodi, angkatan, rombel) VALUES
('Sarjana Terapan Teknik Informatika', 2025, 'C'),
('Sarjana Terapan Teknik Informatika', 2025, 'A');

-- Master Akun Pengguna
INSERT INTO users (username, password, nama, role, id_kelas) VALUES
('admin', '$2a$10$vI8aWBnW3fID.ZQ4/zo1G.q1lPrps.9cRH4ykGyoIxvVU6ZPiU6Vy', 'Administrator Penjaminan Mutu', 'admin', NULL),
('198501012010122001', '$2a$10$7R9f5hHSmxT7mJ1tHnJkUe3qXkPvTjOaWqYyYkPvZjOaWqYyYkPvK', 'RISNA SARI, S.Kom., M.T.I.', 'dosen', NULL),
('197805122005011002', '$2a$10$7R9f5hHSmxT7mJ1tHnJkUe3qXkPvTjOaWqYyYkPvZjOaWqYyYkPvK', 'BAMBANG WARSUTA, S.Kom., M.T.I.', 'dosen', NULL),
('2507411001', '$2a$10$d8K1zG5u2pA5/tY4vX8jOuE7rM9vQ1jZ6sF2tL8xV3bN5mK7lP9wS', 'FARHAN IBRA ANDHIKA', 'mahasiswa', 1),
('2507411025', '$2a$10$d8K1zG5u2pA5/tY4vX8jOuE7rM9vQ1jZ6sF2tL8xV3bN5mK7lP9wS', 'AHMAD FATAH HABIBI', 'mahasiswa', 1);

-- Master Mata Kuliah
INSERT INTO matkul (kode_matkul, nama_matkul, sks) VALUES
('TIK205', 'Sistem Basis Data', 2),
('TIK208', 'Sistem Informasi Manajemen', 2);

-- Periode Aktif
INSERT INTO periode_edom (nama_periode, tahun_ajaran, semester, tanggal_mulai, tanggal_selesai, status_aktif) VALUES
('2025/2026 Genap', 2025, 'Genap', '2026-05-01', '2026-07-31', TRUE);

-- Penugasan Pengampu
INSERT INTO pengampu (id_dosen, id_matkul, id_kelas, id_periode) VALUES
(2, 1, 1, 1), -- Bu Risna mengajar Sistem Basis Data di Angkatan 2025 Rombel C
(3, 2, 1, 1); -- Pak Bambang mengajar SIM di Angkatan 2025 Rombel C

-- Master 27 Instrumen Soal EDOM PNJ (Typo diperbaiki, ENUM lowercase)
INSERT INTO pertanyaan (kategori, pertanyaan) VALUES
-- Kompetensi Profesional (8 Soal)
('profesional', 'Kemampuan memberi contoh relevan dari konsep yang diajarkan berdasarkan Program Studi'),
('profesional', 'Kemampuan menjelaskan keterkaitan bidang/topik yang diajarkan dengan bidang/topik lain'),
('profesional', 'Kemampuan menjelaskan keterkaitan bidang/topik yang diajarkan dengan konteks kehidupan'),
('profesional', 'Penguasaan akan isu-isu mutakhir dalam bidang yang diajarkan (kemutakhiran bahan/referensi kuliah)'),
('profesional', 'Penggunaan hasil-hasil penelitian untuk meningkatkan kualitas perkuliahan'),
('profesional', 'Pelibatan mahasiswa dalam penelitian/kajian dan atau pengembangan/rekayasa/desain yang dilakukan dosen'),
('profesional', 'Kemampuan menggunakan beragam teknologi komunikasi'),
('profesional', 'Kemampuan menjelaskan pokok bahasan/topik secara tepat sesuai Rencana Pembelajaran Semester (RPS)'),

-- Kompetensi Pedagogik (9 Soal)
('pedagogik', 'Kesiapan memberikan kuliah dan/atau praktek/praktikum'),
('pedagogik', 'Keteraturan dan ketertiban penyelenggaraan perkuliahan'),
('pedagogik', 'Kemampuan menghidupkan suasana kelas'),
('pedagogik', 'Kejelasan penyampaian materi dan jawaban terhadap pertanyaan di kelas'),
('pedagogik', 'Pemanfaatan media dan teknologi pembelajaran'),
('pedagogik', 'Keanekaragaman cara pengukuran/penilaian hasil belajar'),
('pedagogik', 'Pemberian umpan balik terhadap tugas/penilaian'),
('pedagogik', 'Kesesuaian materi ujian dan/atau tugas dengan tujuan mata kuliah'),
('pedagogik', 'Kesesuaian nilai yang diberikan dengan hasil belajar'),

-- Kompetensi Kepribadian (6 Soal)
('kepribadian', 'Kearifan dalam mengambil keputusan'),
('kepribadian', 'Satu kata dan tindakan'),
('kepribadian', 'Kemampuan mengendalikan diri dalam berbagai situasi dan kondisi'),
('kepribadian', 'Adil dalam memperlakukan mahasiswa'),
('kepribadian', 'Menjadi contoh dalam bersikap dan berperilaku'),
('kepribadian', 'Kewibawaan sebagai pribadi dosen'),

-- Kompetensi Sosial (4 Soal)
('sosial', 'Kemampuan menyampaikan pendapat'),
('sosial', 'Kemampuan menerima kritik, saran, dan pendapat dari mahasiswa'),
('sosial', 'Mengenal dengan baik mahasiswa yang mengikuti kuliahnya'),
('sosial', 'Toleransi terhadap keberagaman mahasiswa');


UPDATE users SET password = SHA2('admin', 256) WHERE username = 'admin';
UPDATE users SET password = SHA2('dosen', 256) WHERE role = 'dosen';
UPDATE users SET password = SHA2('mhs', 256)   WHERE role = 'mahasiswa';jawaban