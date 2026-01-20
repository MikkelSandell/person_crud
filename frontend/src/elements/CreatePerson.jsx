import React, { useRef, useState } from 'react';

function CreatePerson({ onCreated }) {
  const [username, setUsername] = useState('');
  const [cprPart1, setCprPart1] = useState('');
  const [cprPart2, setCprPart2] = useState('');
  const [profilePicture, setProfilePicture] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [cprError, setCprError] = useState('');
  const cprPart2Ref = useRef(null);

  const isValidCprDate = (ddmmyy) => {
    if (!ddmmyy || ddmmyy.length !== 6) return false;

    const day = parseInt(ddmmyy.substring(0, 2), 10);
    const month = parseInt(ddmmyy.substring(2, 4), 10);
    const year = parseInt(ddmmyy.substring(4, 6), 10);

    // Determine century (00-29 = 2000s, 30-99 = 1900s)
    const fullYear = year <= 29 ? 2000 + year : 1900 + year;

    // Month validation (1-12)
    if (month < 1 || month > 12) return false;

    // Day validation (1-31)
    if (day < 1 || day > 31) return false;

    // More precise validation considering days in month
    const daysInMonth = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];
    
    // Check leap year
    const isLeapYear = (fullYear % 4 === 0 && fullYear % 100 !== 0) || (fullYear % 400 === 0);
    if (isLeapYear) daysInMonth[1] = 29;

    return day <= daysInMonth[month - 1];
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');
    setSuccess('');
    setCprError('');

    const cleanPart1 = cprPart1.trim();
    const cleanPart2 = cprPart2.trim();
    if (cleanPart1.length !== 6 || cleanPart2.length !== 4) {
      setError('CPR must be 6 digits - 4 digits');
      setLoading(false);
      return;
    }

    // Validate CPR date
    if (!isValidCprDate(cleanPart1)) {
      setCprError('Invalid date format in CPR (DDMMYY)');
      setLoading(false);
      return;
    }

    const cpr = `${cleanPart1}-${cleanPart2}`;

    try {
      const res = await fetch('http://localhost:5000/api/person', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, cpr, profilePicture }),
      });

      if (!res.ok) {
        const message = await res.text();
        throw new Error(message || 'Failed to create person');
      }

      setSuccess('Person created');
      setUsername('');
      setCprPart1('');
      setCprPart2('');
      setCprError('');
      setProfilePicture('');
      onCreated?.();
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleFileChange = (e) => {
    const file = e.target.files?.[0];
    if (!file) {
      setProfilePicture('');
      return;
    }

    const reader = new FileReader();
    reader.onloadend = () => {
      setProfilePicture(reader.result?.toString() || '');
    };
    reader.readAsDataURL(file);
  };

  return (
    <div className="card">
      <h2>Create Person</h2>
      <form className="form-vertical" onSubmit={handleSubmit}>
        <label>
          Username
          <input value={username} onChange={(e) => setUsername(e.target.value)} required />
        </label>
        <label>
          CPR
          <div className="cpr-row">
            <input
              inputMode="numeric"
              pattern="[0-9]*"
              maxLength={6}
              placeholder="DDMMYY"
              value={cprPart1}
              onChange={(e) => {
                const sanitized = e.target.value.replace(/\D/g, '').slice(0, 6);
                setCprPart1(sanitized);
                // Clear error when user starts typing
                if (sanitized.length > 0) {
                  setCprError('');
                }
                if (sanitized.length === 6) {
                  cprPart2Ref.current?.focus();
                }
              }}
              className={cprError ? 'input-error' : ''}
              required
            />
            <span className="cpr-separator">-</span>
            <input
              inputMode="numeric"
              pattern="[0-9]*"
              maxLength={4}
              placeholder="XXXX"
              value={cprPart2}
              onChange={(e) => {
                const sanitized = e.target.value.replace(/\D/g, '').slice(0, 4);
                setCprPart2(sanitized);
                // Clear error when user starts typing
                if (sanitized.length > 0) {
                  setCprError('');
                }
              }}
              className={cprError ? 'input-error' : ''}
              ref={cprPart2Ref}
              required
            />
          </div>
          {cprError && <small className="error-message">{cprError}</small>}
        </label>
        <label>
          Profile picture (optional)
          <input type="file" accept="image/*" onChange={handleFileChange} />
          {profilePicture && <small className="muted">Image loaded</small>}
        </label>
        <button type="submit" disabled={loading}>{loading ? 'Saving...' : 'Create'}</button>
      </form>
      {success && <p className="success-message">{success}</p>}
      {error && <p className="error-message">{error}</p>}
    </div>
  );
}

export default CreatePerson;
