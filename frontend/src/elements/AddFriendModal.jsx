import React, { useState, useEffect } from 'react';
import placeholderImg from '../UtilElements/placeholder.svg';

function AddFriendModal({ person, allPersons, onClose, onAddFriend }) {
  const [selectedFriendId, setSelectedFriendId] = useState('');
  const [loading, setLoading] = useState(false);

  const availableFriends = allPersons.filter(
    (p) => p.id !== person.id && !person.friendIds?.includes(p.id)
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
