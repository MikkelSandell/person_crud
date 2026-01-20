export function formatCpr(cpr) {
  if (!cpr) return '';
  const cleaned = cpr.replace(/\D/g, '');
  if (cleaned.length < 10) return cpr;
  return `${cleaned.slice(0, 6)}-${cleaned.slice(6, 10)}`;
}
