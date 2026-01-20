import React, { useState, useEffect } from 'react';
import placeholderImg from '../UtilElements/placeholder.svg';
import { formatCpr } from '../UtilElements/cprFormatter';
import FriendModal from './FriendModal';
import EditModal from './EditModal';

function PersonModal({ person, onClose, onDelete, onAddFriendClick, onFriendClick, onRemoveFriend, onUpdate }) {
  const [isEditing, setIsEditing] = useState(false);

  // Reset editing state when person changes
  useEffect(() => {
    setIsEditing(false);
  }, [person?.id]);

  if (!person) return null;

  const handleDelete = async () => {
    if (!window.confirm(`Delete ${person.username}?`)) return;
    onDelete?.(person.id);
  };

  const handleEditClick = () => {
    setIsEditing(true);
  };

  const handleEditSave = (updated) => {
    onUpdate?.(updated);
    setIsEditing(false);
  };

  const handleEditCancel = () => {
    setIsEditing(false);
  };

  if (isEditing) {
    return <EditModal person={person} onSave={handleEditSave} onCancel={handleEditCancel} />;
  }

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <button className="modal-close" onClick={onClose}>✕</button>

        <div className="modal-header">
          <div className="modal-avatar">
            <img
              src={person.profilePicture || placeholderImg}
              alt={person.username}
            />
          </div>
          <div className="modal-info">
            <div className="modal-title-row">
              <h2>{person.username}</h2>
              <button className="btn-edit" onClick={handleEditClick} title="Edit">
                ✏️
              </button>
            </div>
            <div className="modal-details">
              <div className="detail-row">
                <span className="detail-label">CPR:</span>
                <span className="detail-value">{formatCpr(person.cpr)}</span>
              </div>
              <div className="detail-row">
                <span className="detail-label">Age:</span>
                <span className="detail-value">{person.age ?? '-'}</span>
              </div>
              <div className="detail-row">
                <span className="detail-label">Star Sign:</span>
                <span className="detail-value">{person.starSign || '-'}</span>
              </div>
            </div>
          </div>
        </div>

        <div className="modal-actions">
          <button className="btn-delete" onClick={handleDelete}>
            Delete Person
          </button>
          <button className="btn-add-friend" onClick={() => onAddFriendClick?.(person)}>
            Add Friend
          </button>
        </div>

        <FriendModal
          friendIds={person.friendIds}
          onFriendClick={onFriendClick}
          onRemoveFriend={onRemoveFriend}
        />
      </div>
    </div>
  );
}

export default PersonModal;
