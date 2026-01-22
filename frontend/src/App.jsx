import React, { useEffect, useState, useCallback } from 'react';
import Greetings from './elements/Greetings';
import CreatePerson from './elements/CreatePerson';
import PersonTable from './elements/PersonTable';
import PersonModal from './elements/PersonModal';
import AddFriendModal from './elements/AddFriendModal';

function App() {
  const [persons, setPersons] = useState([]);
  const [personsLoading, setPersonsLoading] = useState(false);
  const [personsError, setPersonsError] = useState('');
  const [selectedPerson, setSelectedPerson] = useState(null);
  const [showAddFriendModal, setShowAddFriendModal] = useState(false);
  const [currentPage, setCurrentPage] = useState(0);
  const [totalPersons, setTotalPersons] = useState(0);
  const pageSize = 5;
  const [sortBy, setSortBy] = useState('username');
  const [sortOrder, setSortOrder] = useState('asc');

  const fetchPersons = useCallback(async (page = 0, sort = 'username', order = 'asc') => {
    setPersonsLoading(true);
    setPersonsError('');
    try {
      const res = await fetch(`http://localhost:5000/api/person?skip=${page * pageSize}&pageSize=${pageSize}&sortBy=${sort}&sortOrder=${order}`);
      if (!res.ok) {
        throw new Error('Failed to load persons');
      }
      const data = await res.json();
      setPersons(data.items);
      setTotalPersons(data.total);
      setCurrentPage(page);
      setSortBy(sort);
      setSortOrder(order);
    } catch (err) {
      setPersonsError(err.message);
    } finally {
      setPersonsLoading(false);
    }
  }, [pageSize]);

  const handleDeletePerson = async (id) => {
    try {
      const res = await fetch(`http://localhost:5000/api/person/${id}`, {
        method: 'DELETE',
      });
      if (!res.ok) throw new Error('Failed to delete');
      setSelectedPerson(null);
      fetchPersons(currentPage, sortBy, sortOrder);
    } catch (err) {
      alert(err.message);
    }
  };

  const handleSort = (column) => {
    if (sortBy === column) {
      const newOrder = sortOrder === 'asc' ? 'desc' : 'asc';
      setSortOrder(newOrder);
      fetchPersons(0, column, newOrder);
    } else {
      setSortBy(column);
      fetchPersons(0, column, 'asc');
    }
  };

  const handleAddFriend = async (personId, friendId) => {
    try {
      const res = await fetch(`http://localhost:5000/api/friend/${personId}/add/${friendId}`, {
        method: 'POST',
      });
      if (!res.ok) throw new Error('Failed to add friend');
      const updated = await res.json();
      setSelectedPerson(updated);
      fetchPersons(currentPage, sortBy, sortOrder);
      setShowAddFriendModal(false);
    } catch (err) {
      alert(err.message);
    }
  };

  const handleRemoveFriend = async (friendId) => {
    if (!selectedPerson) return;
    try {
      const res = await fetch(
        `http://localhost:5000/api/friend/${selectedPerson.id}/remove/${friendId}`,
        { method: 'DELETE' }
      );
      if (!res.ok) throw new Error('Failed to remove friend');
      const updated = await res.json();
      setSelectedPerson(updated);
      fetchPersons(currentPage, sortBy, sortOrder);
    } catch (err) {
      alert(err.message);
    }
  };

  useEffect(() => {
    fetchPersons(0, 'username', 'asc');
  }, [fetchPersons]);

  return (
    <div className="App">
      <h1>Greetings App</h1>
      <Greetings />

      <div className="grid">
        <CreatePerson onCreated={() => fetchPersons(0)} />
        <div>
          <PersonTable
            persons={persons}
            loading={personsLoading}
            error={personsError}
            onRefresh={() => fetchPersons(currentPage, sortBy, sortOrder)}
            onRowClick={setSelectedPerson}
            onSort={handleSort}
            currentSort={sortBy}
            currentOrder={sortOrder}
          />
          {totalPersons > pageSize && (
            <div className="pagination-controls">
              <button
                onClick={() => fetchPersons(currentPage - 1, sortBy, sortOrder)}
                disabled={currentPage === 0}
                className="pagination-btn"
              >
                ← Previous
              </button>
              <span className="pagination-info">
                Page {currentPage + 1} of {Math.ceil(totalPersons / pageSize)}
              </span>
              <button
                onClick={() => fetchPersons(currentPage + 1, sortBy, sortOrder)}
                disabled={(currentPage + 1) * pageSize >= totalPersons}
                className="pagination-btn"
              >
                Next →
              </button>
            </div>
          )}
        </div>
      </div>

      <PersonModal
        person={selectedPerson}
        onClose={() => setSelectedPerson(null)}
        onDelete={handleDeletePerson}
        onAddFriendClick={() => setShowAddFriendModal(true)}
        onFriendClick={setSelectedPerson}
        onRemoveFriend={handleRemoveFriend}
        onUpdate={(updated) => {
          setSelectedPerson(updated);
          fetchPersons(currentPage, sortBy, sortOrder);
        }}
      />

      {showAddFriendModal && selectedPerson && (
        <AddFriendModal
          person={selectedPerson}
          onClose={() => setShowAddFriendModal(false)}
          onAddFriend={handleAddFriend}
        />
      )}
    </div>
  );
}

export default App;
