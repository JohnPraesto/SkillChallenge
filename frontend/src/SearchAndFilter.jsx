import React, { useState, useEffect } from "react";

export function SearchAndFilter({ 
  challenges, 
  onFilteredChallenges, 
  categories = [] 
}) {
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedCategory, setSelectedCategory] = useState("");
  const [sortBy, setSortBy] = useState("newest");
  const [showOnlyOpen, setShowOnlyOpen] = useState(false);

  useEffect(() => {
    let filtered = [...challenges];

    // Sök
    if (searchTerm) {
      filtered = filtered.filter(challenge =>
        challenge.challengeName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        challenge.description?.toLowerCase().includes(searchTerm.toLowerCase()) ||
        challenge.creatorUserName.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Filtrera efter kategori
    if (selectedCategory) {
      filtered = filtered.filter(challenge =>
        challenge.subCategory?.category?.categoryName === selectedCategory
      );
    }

    // Filtrera endast öppna
    if (showOnlyOpen) {
      const now = new Date();
      filtered = filtered.filter(challenge => new Date(challenge.endDate) > now);
    }

    // Sortera
    filtered.sort((a, b) => {
      switch (sortBy) {
        case "newest":
          return new Date(b.createdDate || 0) - new Date(a.createdDate || 0);
        case "oldest":
          return new Date(a.createdDate || 0) - new Date(b.createdDate || 0);
        case "ending-soon":
          return new Date(a.endDate) - new Date(b.endDate);
        case "alphabetical":
          return a.challengeName.localeCompare(b.challengeName);
        default:
          return 0;
      }
    });

    onFilteredChallenges(filtered);
  }, [searchTerm, selectedCategory, sortBy, showOnlyOpen, challenges]);

  return (
    <div className="search-filter-container">
      <div className="search-bar">
        <input
          type="text"
          placeholder="Search challenges..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="form-control search-input"
        />
        <span className="search-icon">🔍</span>
      </div>

      <div className="filter-controls">
        <select
          value={selectedCategory}
          onChange={(e) => setSelectedCategory(e.target.value)}
          className="form-control filter-select"
        >
          <option value="">All Categories</option>
          {categories.map(cat => (
            <option key={cat.categoryId} value={cat.categoryName}>
              {cat.categoryName}
            </option>
          ))}
        </select>

        <select
          value={sortBy}
          onChange={(e) => setSortBy(e.target.value)}
          className="form-control filter-select"
        >
          <option value="newest">Newest First</option>
          <option value="oldest">Oldest First</option>
          <option value="ending-soon">Ending Soon</option>
          <option value="alphabetical">A-Z</option>
        </select>

        <label className="checkbox-label">
          <input
            type="checkbox"
            checked={showOnlyOpen}
            onChange={(e) => setShowOnlyOpen(e.target.checked)}
          />
          Only Open Challenges
        </label>
      </div>
    </div>
  );
}