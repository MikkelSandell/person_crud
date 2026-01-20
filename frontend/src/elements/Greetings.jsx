import React, { useEffect, useState } from 'react';

function Greetings() {
  const [greeting, setGreeting] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [name, setName] = useState('');

  const fetchGreeting = async (userName = '') => {
    setLoading(true);
    setError('');

    try {
      const endpoint = userName
        ? `http://localhost:5000/api/greetings/${userName}`
        : 'http://localhost:5000/api/greetings';

      const response = await fetch(endpoint);

      if (!response.ok) {
        throw new Error('Failed to fetch greeting');
      }

      const data = await response.json();
      setGreeting(data.message);
    } catch (err) {
      setError(err.message);
      setGreeting('');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchGreeting();
  }, []);

  const handleSubmit = (e) => {
    e.preventDefault();
    fetchGreeting(name);
  };

  return (
    <div>
      <form onSubmit={handleSubmit}>
        <input
          type="text"
          placeholder="Enter your name (optional)"
          value={name}
          onChange={(e) => setName(e.target.value)}
        />
        <button type="submit" disabled={loading}>
          {loading ? 'Loading...' : 'Get Greeting'}
        </button>
      </form>

      {greeting && (
        <div className="greeting-message">
          <p>{greeting}</p>
        </div>
      )}

      {error && (
        <div className="error-message">
          <p>Error: {error}</p>
        </div>
      )}
    </div>
  );
}

export default Greetings;
