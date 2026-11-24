import json
import sys

RULES_81 = [
    ("B", "B", "B", "B", 95, "Kinerja sangat unggul di seluruh pilar, layak diberikan apresiasi institusi"),
    ("B", "B", "B", "C", 85, "Pengajaran dan kepribadian unggul, pertahankan dan tingkatkan komunikasi sosial"),
    ("B", "B", "B", "K", 40, "Wajib evaluasi pembinaan komunikasi sosial dan pendekatan terhadap mahasiswa"),
    ("B", "B", "C", "B", 85, "Pengajaran dan interaksi sosial unggul, pertahankan keteladanan sikap"),
    ("B", "B", "C", "C", 70, "Kinerja pengajaran baik dan stabil, pertahankan kedisiplinan mengajar"),
    ("B", "B", "C", "K", 40, "Pengajaran baik namun komunikasi sosial kurang, perlu evaluasi pendekatan kelas"),
    ("B", "B", "K", "B", 40, "Pengajaran unggul namun wajib evaluasi pembinaan kepribadian dan etika dosen"),
    ("B", "B", "K", "C", 40, "Wajib evaluasi pembinaan kepribadian, kedisiplinan waktu, dan keteladanan"),
    ("B", "B", "K", "K", 40, "Wajib evaluasi mendalam terkait etika dan interaksi sosial dengan mahasiswa"),
    ("B", "C", "B", "B", 85, "Pedagogik dan karakter unggul, perbarui referensi kemutakhiran materi"),
    ("B", "C", "B", "C", 75, "Pertahankan teknik mengajar, tingkatkan pengayaan materi perkuliahan"),
    ("B", "C", "B", "K", 40, "Perlu evaluasi komunikasi sosial dan pembaruan materi pembelajaran"),
    ("B", "C", "C", "B", 70, "Kinerja stabil, pertahankan suasana kelas dan tingkatkan materi ajar"),
    ("B", "C", "C", "C", 65, "Kinerja standar, pertahankan konsistensi dan ikuti pelatihan materi berkala"),
    ("B", "C", "C", "K", 40, "Wajib evaluasi interaksi kelas dan peningkatan materi perkuliahan"),
    ("B", "C", "K", "B", 40, "Wajib evaluasi etika mengajar dan kepribadian dosen"),
    ("B", "C", "K", "C", 40, "Perlu evaluasi pembinaan karakter dan pembaruan silabus/materi"),
    ("B", "C", "K", "K", 40, "Wajib evaluasi etika dosen dan pendekatan sosial mahasiswa"),
    ("B", "K", "B", "B", 65, "Metode mengajar baik, namun wajib perbarui bahan ajar yang kurang relevan"),
    ("B", "K", "B", "C", 60, "Perlu pengayaan penguasaan materi perkuliahan secara mandiri"),
    ("B", "K", "B", "K", 40, "Wajib evaluasi penguasaan materi perkuliahan dan komunikasi kelas"),
    ("B", "K", "C", "B", 60, "Perlu pendalaman materi bidang keahlian yang diajarkan"),
    ("B", "K", "C", "C", 40, "Penguasaan materi kurang, wajib evaluasi silabus dan rencana pembelajaran"),
    ("B", "K", "C", "K", 40, "Wajib evaluasi materi dan kemampuan komunikasi mahasiswa"),
    ("B", "K", "K", "B", 40, "Wajib evaluasi mendalam penguasaan materi dan etika dosen"),
    ("B", "K", "K", "C", 40, "Wajib evaluasi materi ajar dan kepribadian mengajar"),
    ("B", "K", "K", "K", 40, "Evaluasi menyeluruh pada materi, karakter, dan komunikasi"),
    ("C", "B", "B", "B", 85, "Materi ajar sangat luas dan mutakhir, pertahankan dan variasikan cara mengajar"),
    ("C", "B", "B", "C", 75, "Materi perkuliahan bagus, pertahankan dan tingkatkan keaktifan kelas"),
    ("C", "B", "B", "K", 40, "Materi bagus namun komunikasi sosial kurang, wajib evaluasi interaksi"),
    ("C", "B", "C", "B", 70, "Kinerja cukup baik, pertahankan dan ikuti pelatihan metodologi pengajaran"),
    ("C", "B", "C", "C", 65, "Kinerja standar memadai, pertahankan dan tingkatkan teknik penyampaian"),
    ("C", "B", "C", "K", 40, "Wajib evaluasi komunikasi sosial dan variasi metode mengajar"),
    ("C", "B", "K", "B", 40, "Materi bagus namun wajib evaluasi pembinaan kepribadian dan etika"),
    ("C", "B", "K", "C", 40, "Wajib evaluasi kepribadian mengajar dan manajemen kelas"),
    ("C", "B", "K", "K", 40, "Wajib evaluasi etika dosen dan hubungan interpersonal dengan mahasiswa"),
    ("C", "C", "B", "B", 70, "Sikap dan hubungan sosial sangat baik, pertahankan dan tingkatkan mutu pengajaran"),
    ("C", "C", "B", "C", 65, "Hubungan mahasiswa baik, pertahankan dan perbaiki kualitas penyampaian materi"),
    ("C", "C", "B", "K", 40, "Perlu evaluasi metode pengajaran dan perbaikan komunikasi sosial"),
    ("C", "C", "C", "B", 65, "Kinerja umum cukup, pertahankan dan ikuti pelatihan pedagogik berkala"),
    ("C", "C", "C", "C", 60, "Kinerja rata-rata standar, pertahankan ketertiban perkuliahan"),
    ("C", "C", "C", "K", 40, "Kinerja cukup namun interaksi mahasiswa kurang, wajib evaluasi"),
    ("C", "C", "K", "B", 40, "Wajib evaluasi kedisiplinan dan pembinaan etika dosen"),
    ("C", "C", "K", "C", 40, "Wajib evaluasi kepribadian dan cara mengajar dosen"),
    ("C", "C", "K", "K", 40, "Wajib evaluasi menyeluruh etika dan komunikasi sosial"),
    ("C", "K", "B", "B", 45, "Penguasaan materi kurang, wajib evaluasi dan perbarui modul kuliah"),
    ("C", "K", "B", "C", 40, "Wajib evaluasi dan pelatihan pengayaan materi bidang studi"),
    ("C", "K", "B", "K", 40, "Wajib evaluasi materi kuliah dan kemampuan interaksi kelas"),
    ("C", "K", "C", "B", 40, "Wajib evaluasi dan penguatan substansi materi keahlian"),
    ("C", "K", "C", "C", 40, "Wajib evaluasi materi dan teknik penyampaian di kelas"),
    ("C", "K", "C", "K", 40, "Wajib evaluasi total materi perkuliahan dan sikap"),
    ("C", "K", "K", "B", 40, "Wajib evaluasi penguasaan materi dan pembinaan kepribadian"),
    ("C", "K", "K", "C", 40, "Wajib evaluasi materi ajar dan etika dosen"),
    ("C", "K", "K", "K", 40, "Wajib evaluasi total aspek akademik dan karakter"),
    ("K", "B", "B", "B", 65, "Materi unggul namun cara mengajar monoton, wajib pelatihan metode pengajaran"),
    ("K", "B", "B", "C", 60, "Kuasai materi tapi cara penyampaian kurang, disarankan pelatihan media"),
    ("K", "B", "B", "K", 40, "Wajib evaluasi metode pengajaran dan perbaikan komunikasi"),
    ("K", "B", "C", "B", 45, "Wajib evaluasi penyusunan RPS dan variasi teknik mengajar"),
    ("K", "B", "C", "C", 40, "Wajib evaluasi pengajaran dan kemampuan menghidupkan kelas"),
    ("K", "B", "C", "K", 40, "Wajib evaluasi pedagogik dan pembinaan komunikasi mahasiswa"),
    ("K", "B", "K", "B", 40, "Wajib evaluasi etika mengajar dan pelatihan pedagogik"),
    ("K", "B", "K", "C", 40, "Wajib evaluasi kepribadian dan keterampilan pedagogik"),
    ("K", "B", "K", "K", 40, "Wajib evaluasi total aspek pengajaran dan etika dosen"),
    ("K", "C", "B", "B", 45, "Hubungan baik namun cara mengajar kurang, wajib pelatihan PEKERTI"),
    ("K", "C", "B", "C", 40, "Wajib evaluasi metode pengajaran dan penyusunan bahan kuliah"),
    ("K", "C", "B", "K", 40, "Wajib evaluasi pedagogik dan pendekatan terhadap mahasiswa"),
    ("K", "C", "C", "B", 40, "Wajib evaluasi dan bimbingan teknis pembelajaran perguruan tinggi"),
    ("K", "C", "C", "C", 40, "Wajib evaluasi dan pelatihan dasar instruksional mengajar"),
    ("K", "C", "C", "K", 40, "Wajib evaluasi pedagogik dan etika komunikasi kelas"),
    ("K", "C", "K", "B", 40, "Wajib evaluasi kepribadian dan pelatihan teknik mengajar"),
    ("K", "C", "K", "C", 40, "Wajib evaluasi pedagogik dan pembinaan karakter"),
    ("K", "C", "K", "K", 40, "Wajib evaluasi intensif kemampuan mengajar dan etika"),
    ("K", "K", "B", "B", 40, "Kepribadian baik namun aspek akademik kurang, wajib evaluasi PEKERTI"),
    ("K", "K", "B", "C", 40, "Wajib evaluasi intensif penguasaan materi dan cara mengajar"),
    ("K", "K", "B", "K", 40, "Wajib evaluasi kemampuan akademik dan komunikasi kelas"),
    ("K", "K", "C", "B", 40, "Wajib evaluasi menyeluruh beban mengajar dan materi"),
    ("K", "K", "C", "C", 40, "Kinerja di bawah standar, wajib evaluasi penugasan mata kuliah"),
    ("K", "K", "C", "K", 40, "Wajib evaluasi mendalam penugasan dan pembinaan dosen"),
    ("K", "K", "K", "B", 40, "Wajib evaluasi total aspek akademik dan etika mengajar"),
    ("K", "K", "K", "C", 40, "Wajib evaluasi total kinerja pengajaran dan kepribadian"),
    ("K", "K", "K", "K", 40, "Kinerja sangat tidak memuaskan, wajib evaluasi khusus pimpinan"),
]


def konversi_ke_skala_100(total_likert, jumlah_pertanyaan):
    if jumlah_pertanyaan <= 0:
        return 0.0
    skor_max = 5 * jumlah_pertanyaan
    return (total_likert / skor_max) * 100


def fuzzifikasi(nilai):
    if nilai <= 40:
        k = 1.0
    elif nilai < 60:
        k = (60 - nilai) / 20
    else:
        k = 0.0

    if nilai <= 40 or nilai >= 80:
        c = 0.0
    elif nilai <= 60:
        c = (nilai - 40) / 20
    else:
        c = (80 - nilai) / 20

    if nilai <= 60:
        b = 0.0
    elif nilai < 80:
        b = (nilai - 60) / 20
    else:
        b = 1.0

    return {"K": k, "C": c, "B": b}


def tentukan_predikat(skor):
    if skor >= 80:
        return "Apresiasi"
    elif skor >= 60:
        return "Pertahankan"
    else:
        return "Evaluasi"


def hitung_sugeno(skor_ped, skor_prof, skor_kep, skor_sos):
    mu_ped = fuzzifikasi(skor_ped)
    mu_prof = fuzzifikasi(skor_prof)
    mu_kep = fuzzifikasi(skor_kep)
    mu_sos = fuzzifikasi(skor_sos)

    total_bobot = 0.0
    total_alpha = 0.0
    aturan_aktif = []

    for ped, prof, kep, sos, z, saran in RULES_81:
        alpha = min(mu_ped[ped], mu_prof[prof], mu_kep[kep], mu_sos[sos])
        if alpha > 0:
            total_bobot += alpha * z
            total_alpha += alpha
            aturan_aktif.append((alpha, z, saran))

    skor_akhir = total_bobot / total_alpha
    predikat = tentukan_predikat(skor_akhir)

    if mu_kep["K"] >= 0.5 or mu_sos["K"] >= 0.5:
        predikat = "Evaluasi"

    alpha_tertinggi = -1.0
    rekomendasi = ""

    for alpha, z, saran in aturan_aktif:
        if tentukan_predikat(z) == predikat and alpha >= alpha_tertinggi:
            alpha_tertinggi = alpha
            rekomendasi = saran

    if rekomendasi == "":
        if predikat == "Apresiasi":
            rekomendasi = "Kinerja secara keseluruhan unggul, layak diberikan apresiasi"
        elif predikat == "Pertahankan":
            rekomendasi = "Kinerja secara keseluruhan baik dan stabil, pertahankan dan tingkatkan aspek yang lemah"
        else:
            rekomendasi = "Kinerja secara keseluruhan perlu dievaluasi dan dibina"

    return round(skor_akhir, 2), predikat, rekomendasi


if __name__ == "__main__":
    if len(sys.argv) > 1 and sys.argv[1] == "--json":
        daftar_input = json.loads(sys.stdin.read())
        hasil_batch = []

        for item in daftar_input:
            ped = konversi_ke_skala_100(item["tot_ped"], item["jml_ped"])
            prof = konversi_ke_skala_100(item["tot_prof"], item["jml_prof"])
            kep = konversi_ke_skala_100(item["tot_kep"], item["jml_kep"])
            sos = konversi_ke_skala_100(item["tot_sos"], item["jml_sos"])
            skor_akhir, predikat, rekomendasi = hitung_sugeno(ped, prof, kep, sos)

            hasil_batch.append({
                "pedagogik": round(ped, 2),
                "profesional": round(prof, 2),
                "kepribadian": round(kep, 2),
                "sosial": round(sos, 2),
                "skor_akhir": skor_akhir,
                "predikat": predikat,
                "rekomendasi": rekomendasi,
            })

        print(json.dumps(hasil_batch))
    else:
        data_dummy = [
            ("Dosen A", 43, 38, 29, 18),
            ("Dosen B", 29, 25, 21, 13),
            ("Dosen C", 38, 33, 13, 12),
            ("Dosen D", 19, 16, 13, 8),
        ]

        for nama, tot_ped, tot_prof, tot_kep, tot_sos in data_dummy:
            ped = konversi_ke_skala_100(tot_ped, 9)
            prof = konversi_ke_skala_100(tot_prof, 8)
            kep = konversi_ke_skala_100(tot_kep, 6)
            sos = konversi_ke_skala_100(tot_sos, 4)

            skor_akhir, predikat, rekomendasi = hitung_sugeno(ped, prof, kep, sos)
            print(f"{nama} | Ped={ped:.1f}, Prof={prof:.1f}, Kep={kep:.1f}, Sos={sos:.1f}")
            print(f"  -> Skor: {skor_akhir} ({predikat}) | {rekomendasi}\n")
