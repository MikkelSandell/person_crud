import React, { useState, useRef } from 'react';
import placeholderImg from '../UtilElements/placeholder.svg';

function EditModal({ person, onSave, onCancel }) {
  const [editUsername, setEditUsername] = useState(person?.username || '');
  const [editProfilePicture, setEditProfilePicture] = useState(person?.profilePicture || '');
  const cleanCpr = person?.cpr?.replace('-', '') || '';
  const [editCprPart1, setEditCprPart1] = useState(cleanCpr.slice(0, 6) || '');
  const [editCprPart2, setEditCprPart2] = useState(cleanCpr.slice(6, 10) || '');
  const cprPart2Ref = useRef(null);
  const fileInputRef = useRef(null);

  const handleCprPart1Change = (e) => {
    const value = e.target.value.replace(/\D/g, '').slice(0, 6);
    setEditCprPart1(value);
    if (value.length === 6) {
      cprPart2Ref.current?.focus();
    }
  };

  const handleCprPart2Change = (e) => {
    const value = e.target.value.replace(/\D/g, '').slice(0, 4);
    setEditCprPart2(value);
  };

  const handleFileChange = (e) => {
    const file = e.target.files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onloadend = () => {
      setEditProfilePicture(reader.result?.toString() || '');
    };
    reader.readAsDataURL(file);
  };

  const handleSave = async () => {
    try {
      const fullCpr = editCprPart1 + editCprPart2;
      const res = await fetch(`http://localhost:5000/api/person/${person.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          username: editUsername,
          cpr: fullCpr,
          profilePicture: editProfilePicture
        })
      });
      if (!res.ok) throw new Error('Failed to update person');
      const updated = await res.json();
      onSave?.(updated);
    } catch (err) {
      alert(err.message);
    }
  };

  return (
    <div className="modal-overlay" onClick={onCancel}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onCancel}>✕</button>

        <div className="modal-header">
          <div className="modal-avatar">
            <>
              <input
                ref={fileInputRef}
                type="file"
                accept="image/*"
                onChange={handleFileChange}
                style={{ display: 'none' }}
              />
              <div
                className="modal-avatar-edit-overlay"
                onClick={() => fileInputRef.current?.click()}
              >
                📷
              </div>
            </>
            <img
              src={editProfilePicture || placeholderImg}
              alt={editUsername}
            />
          </div>
          <div className="modal-info">
            <div className="modal-title-row">
              <input
                type="text"
                value={editUsername}
                onChange={(e) => setEditUsername(e.target.value)}
                className="modal-username-edit"
                placeholder="Username"
              />
            </div>
            <div className="modal-details">
              <div className="detail-row">
                <span className="detail-label">CPR:</span>
                <div className="cpr-row" style={{ flex: 1, display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <input
                    type="text"
                    value={editCprPart1}
                    onChange={handleCprPart1Change}
                    maxLength="6"
                    placeholder="DDMMYY"
                    style={{ flexBasis: '70px', padding: '6px', border: '1px solid #ddd', borderRadius: '4px' }}
                  />
                  <span className="cpr-separator">-</span>
                  <input
                    ref={cprPart2Ref}
                    type="text"
                    value={editCprPart2}
                    onChange={handleCprPart2Change}
                    maxLength="4"
                    placeholder="XXXX"
                    style={{ flexBasis: '80px', padding: '6px', border: '1px solid #ddd', borderRadius: '4px' }}
                  />
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="modal-edit-actions">
          <button className="btn-cancel" onClick={onCancel}>
            Cancel
          </button>
          <button className="btn-save" onClick={handleSave}>
            Save
          </button>
        </div>
      </div>
    </div>
  );
}

export default EditModal;
