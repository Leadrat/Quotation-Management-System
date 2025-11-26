"use client";
import { useState, useEffect } from "react";
import { ProductCategoriesApi } from "@/lib/api";
import type { ProductCategory } from "@/types/products";
import Button from "@/components/tailadmin/ui/button/Button";
import { FaChevronRight, FaChevronDown, FaFolder, FaFolderOpen } from "react-icons/fa";

interface ProductCategoryTreeProps {
  onCategorySelect?: (category: ProductCategory) => void;
  selectedCategoryId?: string;
  showActions?: boolean;
  onEdit?: (category: ProductCategory) => void;
  onDelete?: (category: ProductCategory) => void;
}

interface CategoryNode extends ProductCategory {
  children: CategoryNode[];
  expanded?: boolean;
}

export default function ProductCategoryTree({
  onCategorySelect,
  selectedCategoryId,
  showActions = false,
  onEdit,
  onDelete,
}: ProductCategoryTreeProps) {
  const [categories, setCategories] = useState<CategoryNode[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadCategories();
  }, []);

  async function loadCategories() {
    setLoading(true);
    setError(null);
    try {
      const res = await ProductCategoriesApi.list({ isActive: true });
      const allCategories = res.data || [];
      
      // Build tree structure
      const categoryMap = new Map<string, CategoryNode>();
      const rootCategories: CategoryNode[] = [];

      // First pass: create all nodes
      allCategories.forEach((cat) => {
        categoryMap.set(cat.categoryId, { ...cat, children: [], expanded: false });
      });

      // Second pass: build tree
      allCategories.forEach((cat) => {
        const node = categoryMap.get(cat.categoryId)!;
        if (cat.parentCategoryId) {
          const parent = categoryMap.get(cat.parentCategoryId);
          if (parent) {
            parent.children.push(node);
          } else {
            rootCategories.push(node);
          }
        } else {
          rootCategories.push(node);
        }
      });

      setCategories(rootCategories);
    } catch (e: any) {
      setError(e.message || "Failed to load categories");
    } finally {
      setLoading(false);
    }
  }

  const toggleExpand = (categoryId: string) => {
    const toggleNode = (nodes: CategoryNode[]): CategoryNode[] => {
      return nodes.map((node) => {
        if (node.categoryId === categoryId) {
          return { ...node, expanded: !node.expanded };
        }
        if (node.children.length > 0) {
          return { ...node, children: toggleNode(node.children) };
        }
        return node;
      });
    };
    setCategories(toggleNode(categories));
  };

  const renderNode = (node: CategoryNode, level: number = 0): React.ReactNode => {
    const isSelected = selectedCategoryId === node.categoryId;
    const hasChildren = node.children.length > 0;

    return (
      <div key={node.categoryId} className="select-none">
        <div
          className={`flex items-center gap-2 px-2 py-1.5 rounded hover:bg-gray-100 dark:hover:bg-gray-800 ${
            isSelected ? "bg-primary/10 border-l-2 border-primary" : ""
          }`}
          style={{ paddingLeft: `${level * 1.5 + 0.5}rem` }}
        >
          {hasChildren ? (
            <button
              type="button"
              onClick={() => toggleExpand(node.categoryId)}
              className="p-1 hover:bg-gray-200 dark:hover:bg-gray-700 rounded"
            >
              {node.expanded ? (
                <FaChevronDown className="w-3 h-3" />
              ) : (
                <FaChevronRight className="w-3 h-3" />
              )}
            </button>
          ) : (
            <span className="w-5" />
          )}
          <span className="mr-2">
            {node.expanded ? (
              <FaFolderOpen className="w-4 h-4 text-primary" />
            ) : (
              <FaFolder className="w-4 h-4 text-gray-400" />
            )}
          </span>
          <button
            type="button"
            onClick={() => onCategorySelect?.(node)}
            className="flex-1 text-left hover:text-primary"
          >
            <span className="font-medium">{node.categoryName}</span>
            <span className="ml-2 text-xs text-gray-500">({node.categoryCode})</span>
          </button>
          {showActions && (
            <div className="flex gap-1">
              {onEdit && (
                <Button
                  size="sm"
                  variant="outline"
                  onClick={(e) => {
                    e.stopPropagation();
                    onEdit(node);
                  }}
                  className="text-xs px-2 py-1"
                >
                  Edit
                </Button>
              )}
              {onDelete && (
                <Button
                  size="sm"
                  variant="outline"
                  onClick={(e) => {
                    e.stopPropagation();
                    if (confirm(`Delete category "${node.categoryName}"?`)) {
                      onDelete(node);
                    }
                  }}
                  className="text-xs px-2 py-1 text-red-500 hover:text-red-700"
                >
                  Delete
                </Button>
              )}
            </div>
          )}
        </div>
        {hasChildren && node.expanded && (
          <div>{node.children.map((child) => renderNode(child, level + 1))}</div>
        )}
      </div>
    );
  };

  if (loading) {
    return <div className="py-4 text-center text-gray-500">Loading categories...</div>;
  }

  if (error) {
    return (
      <div className="py-4 text-center text-red-500">
        {error}
        <Button onClick={loadCategories} className="mt-2" size="sm">
          Retry
        </Button>
      </div>
    );
  }

  if (categories.length === 0) {
    return <div className="py-4 text-center text-gray-500">No categories found</div>;
  }

  return (
    <div className="border border-stroke rounded-lg p-2 dark:border-strokedark">
      {categories.map((node) => renderNode(node))}
    </div>
  );
}

