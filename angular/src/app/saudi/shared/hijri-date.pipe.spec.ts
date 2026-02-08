import { HijriDatePipe } from './hijri-date.pipe';

describe('HijriDatePipe', () => {
  let pipe: HijriDatePipe;

  beforeEach(() => {
    pipe = new HijriDatePipe();
  });

  it('should create the pipe', () => {
    expect(pipe).toBeTruthy();
  });

  it('should return empty string for null', () => {
    expect(pipe.transform(null)).toBe('');
  });

  it('should return empty string for undefined', () => {
    expect(pipe.transform(undefined)).toBe('');
  });

  it('should return empty string for invalid date string', () => {
    expect(pipe.transform('not-a-date')).toBe('');
  });

  it('should format a valid Date object', () => {
    const date = new Date('2024-12-14');
    const result = pipe.transform(date, 'full', 'ar');

    expect(result).toBeTruthy();
    expect(result.length).toBeGreaterThan(0);
  });

  it('should format a valid date string', () => {
    const result = pipe.transform('2024-12-14', 'full', 'en');

    expect(result).toBeTruthy();
    expect(result.length).toBeGreaterThan(0);
  });

  it('should support short format', () => {
    const fullResult = pipe.transform('2024-12-14', 'full', 'en');
    const shortResult = pipe.transform('2024-12-14', 'short', 'en');

    expect(shortResult).toBeTruthy();
    expect(shortResult.length).toBeGreaterThan(0);
    // Short format should typically be shorter or use numeric month
    expect(shortResult).not.toBe(fullResult);
  });
});
