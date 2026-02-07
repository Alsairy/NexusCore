import { Pipe, PipeTransform } from '@angular/core';

// Hijri month names in Arabic
const HIJRI_MONTHS_AR = [
  'محرّم', 'صفر', 'ربيع الأوّل', 'ربيع الآخر',
  'جمادى الأولى', 'جمادى الآخرة', 'رجب', 'شعبان',
  'رمضان', 'شوّال', 'ذو القعدة', 'ذو الحجّة'
];

const HIJRI_MONTHS_EN = [
  'Muharram', 'Safar', 'Rabi\' al-Awwal', 'Rabi\' al-Thani',
  'Jumada al-Ula', 'Jumada al-Akhirah', 'Rajab', 'Sha\'ban',
  'Ramadan', 'Shawwal', 'Dhul Qi\'dah', 'Dhul Hijjah'
];

@Pipe({
  name: 'hijriDate',
  standalone: true,
})
export class HijriDatePipe implements PipeTransform {
  transform(value: Date | string | null | undefined, format: string = 'full', locale: string = 'ar'): string {
    if (!value) return '';

    const date = typeof value === 'string' ? new Date(value) : value;
    if (isNaN(date.getTime())) return '';

    try {
      const hijriFormatter = new Intl.DateTimeFormat(`${locale}-SA-u-ca-islamic-umalqura`, {
        day: 'numeric',
        month: 'long',
        year: 'numeric',
      });

      if (format === 'short') {
        const shortFormatter = new Intl.DateTimeFormat(`${locale}-SA-u-ca-islamic-umalqura`, {
          day: 'numeric',
          month: 'numeric',
          year: 'numeric',
        });
        return shortFormatter.format(date);
      }

      return hijriFormatter.format(date);
    } catch {
      return value?.toString() ?? '';
    }
  }
}
