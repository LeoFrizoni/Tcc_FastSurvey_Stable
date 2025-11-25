export function cn(...classes) {
  return classes
    .flatMap((cls) => {
      if (!cls) return [];
      return Array.isArray(cls) ? cls : [cls];
    })
    .filter(Boolean)
    .join(' ');
}
