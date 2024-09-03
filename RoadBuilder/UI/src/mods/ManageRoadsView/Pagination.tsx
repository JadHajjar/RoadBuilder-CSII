import { Button, Scrollable } from "cs2/ui";
import { LaneListItem } from "../Components/LaneListItem/LaneListItem";
import styles from "./Pagination.module.scss";
import { useValue } from "cs2/api";
import {
  DiscoverCurrentPage$,
  DiscoverMaxPages$,
  setDiscoverPage,
} from "mods/bindings";

export const Pagination = () => {
    const range = 2;
    const currentPage = useValue(DiscoverCurrentPage$);
    const maxPages = useValue(DiscoverMaxPages$);
  const getPageNumbers = () => {
    const pages = [];
    const startPage = Math.max(1, currentPage - range);
    const endPage = Math.min(maxPages, currentPage + range);

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }

    return pages;
  };

  const pageNumbers = getPageNumbers();

  return (
    <div className={styles.pagination}>
      <ul>
        {currentPage > 1 && (
          <li>
            <Button style="flat" onSelect={() => setDiscoverPage(currentPage - 1)}>
              <img style={{maskImage: "url()"}}>
            </Button>
          </li>
        )}
        {pageNumbers.map((page) => (
          <li key={page} className={page === currentPage ? 'active' : ''}>
            <Button style="flat" onSelect={() => setDiscoverPage(page)}><span>{page}</span></Button>
          </li>
        ))}
        {currentPage < maxPages && (
          <li>
            <Button style="flat" onSelect={() => setDiscoverPage(currentPage + 1)}>
              Next
            </Button>
          </li>
        )}
        {currentPage + range < maxPages && (
          <li>
            <div className={styles.dots}>
            </div>
          </li>
          <li>
            <Button style="flat" onSelect={() => setDiscoverPage(maxPages)}>
              <span>{maxPages}</span>
            </Button>
          </li>
        )}
      </ul>
    </div>
  );
}