import React, { useEffect, useState } from 'react';
import placeholderImg from '../UtilElements/placeholder.svg';

function FriendModal({ friendIds, onFriendClick, onRemoveFriend }) {
  const [friends, setFriends] = useState([]);

  useEffect(() => {
    if (friendIds?.length > 0) {
      fetchFriends();
    } else {
      setFriends([]);
    }
  }, [friendIds]);

  const fetchFriends = async () => {
    try {
      const friendPromises = friendIds.map((friendId) =>
        fetch(`http://localhost:5000/api/person/${friendId}`).then((res) =>
          res.ok ? res.json() : null
        )
      );
      const friendData = await Promise.all(friendPromises);
      setFriends(friendData.filter((f) => f !== null));
    } catch (err) {
      console.error('Failed to load friends:', err);
    }
  };

  const handleRemoveFriend = (friend) => {
    if (!window.confirm(`Delete friend ${friend.username}?`)) return;
    onRemoveFriend?.(friend.id);
  };

  if (!friendIds || friendIds.length === 0) return null;

  return (
    <div className="modal-friends">
      <h3>Friends ({friends.length})</h3>
      <div className="friends-list">
        {friends.map((friend) => (
          <div
            key={friend.id}
            className="friend-card"
          >
            <div
              className="friend-card-content"
              onClick={() => onFriendClick?.(friend)}
              style={{ cursor: 'pointer', flex: 1 }}
            >
              <img
                src={friend.profilePicture || placeholderImg}
                alt={friend.username}
                className="friend-avatar"
              />
              <span className="friend-name">{friend.username}</span>
            </div>
            <button
              className="friend-delete-btn"
              onClick={() => handleRemoveFriend(friend)}
              title="Delete friend"
            >
              ✕
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}

export default FriendModal;
