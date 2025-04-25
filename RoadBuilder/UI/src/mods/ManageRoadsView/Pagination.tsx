import { Button, Scrollable } from "cs2/ui";
import { LaneListItem } from "../Components/LaneListItem/LaneListItem";
import styles from "./Pagination.module.scss";
import { useValue } from "cs2/api";
import { DiscoverCurrentPage$, DiscoverMaxPages$ } from "mods/bindings";
import classNames from "classnames";

export const Pagination = (props: { setPage: (page: number) => void }) => {
  const range = 2;
  const currentPage = useValue(DiscoverCurrentPage$);
  const maxPages = useValue(DiscoverMaxPages$);
  const getPageNumbers = () => {
    const pages = [];
    const startPage = Math.max(1, currentPage - range);
    const endPage = Math.min(maxPages, currentPage + range);

    for (let i = currentPage - range; i <= currentPage + range; i++) {
      pages.push(i > maxPages ? -1 : i);
    }

    return pages;
  };

  const pageNumbers = getPageNumbers();

  return (
    <div className={styles.pagination}>
      <ul>
        <li className={currentPage <= 3 && styles.hidden}>
          <Button variant="flat" onSelect={() => props.setPage(1)}>
            <span>1</span>
          </Button>
        </li>
        <li className={currentPage <= 3 && styles.hidden}>
          <div className={styles.dots}>
            <img src="coui://roadbuildericons/RB_Dots.svg" />
          </div>
        </li>
        <li>
          <Button
            variant="flat"
            disabled={currentPage <= 1}
            onSelect={() => props.setPage(currentPage - 1)}
            className={classNames(styles.arrow, currentPage <= 1 && styles.disabled)}
          >
            <img style={{ maskImage: "url(Media/Glyphs/ArrowLeft.svg)" }} />
          </Button>
        </li>
        {pageNumbers.map((page) => (
          <li>
            {page <= 0 ? (
              <div className={styles.dots} />
            ) : (
              <Button variant="flat" className={page === currentPage && styles.selected} onSelect={() => props.setPage(page)}>
                <span>{page}</span>
              </Button>
            )}
          </li>
        ))}
        <li>
          <Button
            variant="flat"
            disabled={currentPage >= maxPages}
            onSelect={() => props.setPage(currentPage + 1)}
            className={classNames(styles.arrow, currentPage >= maxPages && styles.disabled)}
          >
            <img style={{ maskImage: "url(Media/Glyphs/ArrowRight.svg)" }} />
          </Button>
        </li>
        <li className={currentPage + range >= maxPages && styles.hidden}>
          <div className={styles.dots}>
            <img src="coui://roadbuildericons/RB_Dots.svg" />
          </div>
        </li>
        <li className={currentPage + range >= maxPages && styles.hidden}>
          <Button variant="flat" onSelect={() => props.setPage(maxPages)}>
            <span>{maxPages}</span>
          </Button>
        </li>
      </ul>
    </div>
  );
};
