export const Card = ({ children, className = '', onClick }) => {
  return (
    <div
      className={`card p-6 ${onClick ? 'cursor-pointer' : ''} ${className}`}
      onClick={onClick}
    >
      {children}
    </div>
  );
};
