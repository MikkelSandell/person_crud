import React from 'react';
import placeholderImg from '../UtilElements/placeholder.svg';
import { formatCpr } from '../UtilElements/cprFormatter';

function PersonTable({ persons, loading, error, onRefresh, onRowClick, onSort, currentSort, currentOrder }) {
  const getSortIndicator = (column) => {
    if (currentSort !== column) return '';
    return currentOrder === 'asc' ? ' ▲' : ' ▼';
  };
  return (
    <div className="card">
      <div className="table-header">
        <h2>Persons</h2>
        <button type="button" onClick={onRefresh} disabled={loading}>
          {loading ? 'Loading...' : 'Refresh'}
        </button>
      </div>
      {error && <p className="error-message">{error}</p>}
      <div className="table-wrapper">
        <table>
          <thead>
            <tr>
              <th onClick={() => onSort?.('username')} style={{ cursor: 'pointer', userSelect: 'none' }}>
                Username{getSortIndicator('username')}
              </th>
              <th onClick={() => onSort?.('cpr')} style={{ cursor: 'pointer', userSelect: 'none' }}>
                CPR{getSortIndicator('cpr')}
              </th>
              <th onClick={() => onSort?.('age')} style={{ cursor: 'pointer', userSelect: 'none' }}>
                Age{getSortIndicator('age')}
              </th>
              <th onClick={() => onSort?.('starsign')} style={{ cursor: 'pointer', userSelect: 'none' }}>
                Star Sign{getSortIndicator('starsign')}
              </th>
              <th onClick={() => onSort?.('profilepicture')} style={{ cursor: 'pointer', userSelect: 'none' }}>
                Profile Picture{getSortIndicator('profilepicture')}
              </th>
            </tr>
          </thead>
          <tbody>
            {persons.length === 0 && (
              <tr>
                <td colSpan="5" className="muted">No persons yet</td>
              </tr>
            )}
            {persons.map((p) => (
              <tr key={p.id} onClick={() => onRowClick?.(p)} className="clickable-row">
                <td>{p.username}</td>
                <td>{formatCpr(p.cpr)}</td>
                <td>{p.age ?? '-'}</td>
                <td>{p.starSign || '-'}</td>
                <td>
                  <img
                    src={p.profilePicture || placeholderImg}
                    alt={p.profilePicture ? `${p.username}` : 'placeholder'}
                    className="avatar"
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

export default PersonTable;
