import  { useState, useRef, useEffect } from 'react';
import PropTypes from 'prop-types';
import './FilterButton.css';

const OPTIONS = ['All', 'TCP', 'UDP', 'ICMP',"ARP"];

/**
 * A dropdown button component for selecting a filter option.
 * @param {Object} props
 * @param {string} props.selected The currently selected filter option.
 * @param {function} props.onChange Called when the user selects a new filter option.
 * @returns {ReactElement} The FilterButton component.
 */
export default function FilterButton({ selected, onChange }) {
  const [open, setOpen] = useState(false);
  const ref = useRef(null);

  // close when clicking outside
  useEffect(() => {
    function onClick(e) {
      if (ref.current && !ref.current.contains(e.target)) {
        setOpen(false);
      }
    }
    document.addEventListener('mousedown', onClick);
    return () => document.removeEventListener('mousedown', onClick);
  }, []);

  return (
    <div ref={ref} className="filter-button-container">
      <button
        className="Button"
        onClick={() => setOpen(o => !o)}
      >
        {selected} <span className="arrow">▾</span>
      </button>

      {open && (
        <ul className="ListParent">
          {OPTIONS.map(opt => (
            <li
              key={opt}
              className={`ListItem${opt === selected ? ' selected' : ''}`}
              onClick={() => {
                onChange(opt);
                setOpen(false);
              }}
            >
              {opt}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}

FilterButton.propTypes = {
  selected: PropTypes.string.isRequired,
  onChange: PropTypes.func.isRequired,
};
