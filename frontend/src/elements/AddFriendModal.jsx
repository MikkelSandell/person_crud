import React, { useState, useEffect } from 'react';
import placeholderImg from '../UtilElements/placeholder.svg';

function AddFriendModal({ person, onClose, onAddFriend }) {
  const [selectedFriendId, setSelectedFriendId] = useState('');
  const [loading, setLoading] = useState(false);
  const [allPersons, setAllPersons] = useState([]);
  const [allPersonsLoading, setAllPersonsLoading] = useState(false);
  const [allPersonsError, setAllPersonsError] = useState('');

  // Load all persons so selection isn't limited to the current page
  useEffect(() => {
    const loadAll = async () => {
      setAllPersonsLoading(true);
      setAllPersonsError('');
      try {
        // Fetch with a large pageSize to cover typical datasets
        const res = await fetch('http://localhost:5000/api/person?skip=0&pageSize=1000&sortBy=username&sortOrder=asc');
        if (!res.ok) throw new Error('Failed to load people');
        const data = await res.json();
        setAllPersons(data.items || []);
      } catch (err) {
        setAllPersonsError(err.message);
      } finally {
        setAllPersonsLoading(false);
      }
    };

    loadAll();
  }, []);

  const availableFriends = allPersons.filter(
    (p) => p.id !== person.id && !(person.friendIds || []).includes(p.id)
  );

  const handleAdd = async () => {
    if (!selectedFriendId) return;
    setLoading(true);
    try {
      await onAddFriend(person.id, selectedFriendId);
      onClose();
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>✕</button>
        <h2>Add Friend</h2>
        <p className="muted">Select a person to add as a friend</p>
        <select
          value={selectedFriendId}
          onChange={(e) => setSelectedFriendId(e.target.value)}
          style={{ width: '100%', padding: '8px', marginBottom: '16px' }}
        >
          <option value="">-- Select --</option>
          {availableFriends.map((p) => (
            <option key={p.id} value={p.id}>
              {p.username}
            </option>
          ))}
        </select>
        {availableFriends.length === 0 && (
          <p className="muted">No more friends to add</p>
        )}
        <div style={{ display: 'flex', gap: '8px' }}>
          <button
            onClick={handleAdd}
            disabled={loading || !selectedFriendId || availableFriends.length === 0}
            style={{
              flex: 1,
              padding: '10px',
              backgroundColor: '#007bff',
              color: 'white',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer',
            }}
          >
          {allPersonsError && <p className="error-message">{allPersonsError}</p>}
            {loading ? 'Adding...' : 'Add'}
          </button>
          <button
            onClick={onClose}
            style={{
              flex: 1,
              padding: '10px',
              backgroundColor: '#6b7280',
              color: 'white',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer',
            }}
          >
            Cancel
          </button>
        </div>
      </div>
    </div>
  );
}

export default AddFriendModal;
