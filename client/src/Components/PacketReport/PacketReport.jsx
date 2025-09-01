/* eslint-disable no-unused-vars */
/* eslint-disable react-hooks/exhaustive-deps */
import { useState, useEffect } from 'react';
import './PacketReport.css';
import axios from 'axios';
import PacketPaper from '../PacketPaper/PacketPaper';
import Loading from "../Logo/Loading";
import Button from '../Button/Button';

/**
 * PacketReport is a React component that displays a paginated list of packets.
 * It fetches packets from the server when the user navigates to a new page, and
 * caches the results to avoid unnecessary re-fetches.
 *
 * @param {Object} props - The component props.
 * @param {string} [props.userId] - The ID of the user whose packets to display.
 *
 * @returns {ReactElement} The packet report component.
 */
function PacketReport() {
  const PAGE_SIZE = 10;

  const [packets, setPackets] = useState([]);
  const [total, setTotal] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [pageNumber, setPageNumber] = useState(1);
  const [hasMore, setHasMore] = useState(true);
  const [cachedPages, setCachedPages] = useState({}); // { [pageNumber]: { items, total, pageSize, hasMore } }

  const [userId] = useState(
    localStorage.getItem('user') ? JSON.parse(localStorage.getItem('user')).id : ''
  );

  // If user changes, reset cache and go to page 1
  useEffect(() => {
    setCachedPages({});
    setPageNumber(1);
    setTotal(null);
  }, [userId]);

  useEffect(() => {
    // 1) Serve from cache if present
    const cached = cachedPages[pageNumber];
    if (cached) {
      setPackets(cached.items);
      setTotal(cached.total ?? null);

      const ps = cached.pageSize ?? PAGE_SIZE;
      const totalPages = cached.total != null ? Math.ceil(cached.total / ps) : null;
      setHasMore(
        cached.hasMore != null
          ? cached.hasMore
          : (totalPages != null ? pageNumber < totalPages : cached.items.length === ps)
      );

      setLoading(false); // no network, instant render
      console.log('Serving page', pageNumber, 'from cache');
      return;
    }

    // 2) Otherwise fetch and then cache
    const fetchPackets = async () => {
      setLoading(true);
      setError(null);
      try {
        const token = localStorage.getItem('X-Auth-Token');
        const url = `http://localhost:5256/api/user/packets?userId=${encodeURIComponent(
          userId
        )}&pageNumber=${pageNumber}&pageSize=${PAGE_SIZE}`;
        const { data, status } = await axios.get(url, { headers: { 'X-Auth-Token': token } });

        if (status === 200) {
          const items = data.items ?? [];
          const totalFromServer = data.total ?? null;
          const pageSizeFromServer = data.pageSize ?? PAGE_SIZE;

          const totalPages =
            totalFromServer != null ? Math.ceil(totalFromServer / pageSizeFromServer) : null;

          const nextHasMore =
            totalPages != null ? pageNumber < totalPages : items.length === pageSizeFromServer;

          setPackets(items);
          setTotal(totalFromServer);
          setHasMore(nextHasMore);

          // Cache this page
          setCachedPages(prev => ({
            ...prev,
            [pageNumber]: {
              items,
              total: totalFromServer,
              pageSize: pageSizeFromServer,
              hasMore: nextHasMore,
            },
          }));
        } else {
          console.warn('Unexpected status:', status);
        }
      } catch (err) {
        console.error('Error fetching packets:', err);
        setError(err);
      } finally {
        setLoading(false);
      }
    };

    if (userId) fetchPackets();
  }, [pageNumber, userId]); // (intentionally not depending on cachedPages)

  const onPrev = () => {
    if (pageNumber === 1) return alert('You are on the first page');
    setPageNumber(p => p - 1);
  };

  const onNext = () => {
    if (!hasMore) return alert('No more packets');
    setPageNumber(p => p + 1);
  };

  const totalPages = total != null ? Math.ceil(total / PAGE_SIZE) : null;
  const showNoMore =
    (totalPages != null && pageNumber > totalPages) ||
    (total == null && packets.length === 0 && pageNumber > 1);

  if (loading) {
    return (
      <div className="report-container">
        <h2>Packet Report</h2>
        <div className="no-packets"><h2>Loading packets…</h2></div>
        <Loading />
      </div>
    );
  }

  if (error) {
    return (
      <div className="report-container">
        <h2>Packet Report</h2>
        <div className="no-packets"><h2>Failed to load packets.</h2></div>
      </div>
    );
  }

  return (
    <div className="packet-report">
      <div className="button-container">
        <Button content="Previous Page" action={onPrev} />
      </div>

      <div className="report-container">
        <h2>Packet Report</h2>
        <h4>
          Page {pageNumber}
          {total !== null && <> • Total: {total} • Pages: {Math.max(1, Math.ceil(total / PAGE_SIZE))}</>}
        </h4>

        {showNoMore ? (
          <div className="no-packets"><h2>No More Packets</h2></div>
        ) : packets.length > 0 ? (
          packets.map((packet, i) => <PacketPaper key={i} packet={packet} />)
        ) : (
          <div className="no-packets"><h2>No Packets Found</h2></div>
        )}
      </div>

      <div className="button-container">
        <Button content="Next Page" action={onNext} status="action" />
      </div>
    </div>
  );
}

export default PacketReport;
