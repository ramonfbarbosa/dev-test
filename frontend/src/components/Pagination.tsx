import React from "react";
import { Button, Form } from "react-bootstrap";

const PAGE_SIZE_OPTIONS = [5, 10, 25, 50];

interface PaginationProps {
    currentPage: number;
    totalPages: number;
    totalCount: number;
    pageSize: number;
    onPageChange: (page: number) => void;
    onPageSizeChange: (pageSize: number) => void;
}

const Pagination: React.FC<PaginationProps> = ({
    currentPage,
    totalPages,
    totalCount,
    pageSize,
    onPageChange,
    onPageSizeChange,
}) => {
    const getPageNumbers = (): (number | "...")[] => {
        const pages: (number | "...")[] = [];
        const maxVisible = 5;

        if (totalPages <= maxVisible + 2) {
            for (let i = 1; i <= totalPages; i++) pages.push(i);
            return pages;
        }

        pages.push(1);

        const start = Math.max(2, currentPage - 1);
        const end = Math.min(totalPages - 1, currentPage + 1);

        if (start > 2) pages.push("...");
        for (let i = start; i <= end; i++) pages.push(i);
        if (end < totalPages - 1) pages.push("...");

        pages.push(totalPages);
        return pages;
    };

    const from = totalCount === 0 ? 0 : (currentPage - 1) * pageSize + 1;
    const to = Math.min(currentPage * pageSize, totalCount);

    return (
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", padding: "12px 20px", flexWrap: "wrap", gap: 8 }}>
            <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                <small className="text-muted">
                    Exibindo {from}–{to} de {totalCount} registro{totalCount !== 1 ? "s" : ""}
                </small>
                <Form.Select
                    size="sm"
                    value={pageSize}
                    onChange={(e) => onPageSizeChange(Number(e.target.value))}
                    style={{ width: "auto" }}
                >
                    {PAGE_SIZE_OPTIONS.map((size) => (
                        <option key={size} value={size}>{size} por página</option>
                    ))}
                </Form.Select>
            </div>
            {totalPages > 1 && (
                <div style={{ display: "flex", gap: 4, alignItems: "center" }}>
                    <Button
                        variant="outline-secondary"
                        size="sm"
                        disabled={currentPage <= 1}
                        onClick={() => onPageChange(currentPage - 1)}
                    >
                        ‹
                    </Button>
                    {getPageNumbers().map((page, index) =>
                        page === "..." ? (
                            <span key={`ellipsis-${index}`} style={{ padding: "0 6px" }}>...</span>
                        ) : (
                            <Button
                                key={page}
                                variant={page === currentPage ? "primary" : "outline-secondary"}
                                size="sm"
                                onClick={() => onPageChange(page)}
                            >
                                {page}
                            </Button>
                        )
                    )}
                    <Button
                        variant="outline-secondary"
                        size="sm"
                        disabled={currentPage >= totalPages}
                        onClick={() => onPageChange(currentPage + 1)}
                    >
                        ›
                    </Button>
                </div>
            )}
        </div>
    );
};

export default Pagination;
