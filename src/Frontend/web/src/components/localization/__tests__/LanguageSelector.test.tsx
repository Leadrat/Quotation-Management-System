import { render, screen, waitFor } from "@testing-library/react";
import { LanguageSelector } from "../LanguageSelector";
import { LocalizationApi } from "../../../lib/api";

jest.mock("../../../lib/api");

describe("LanguageSelector", () => {
  beforeEach(() => {
    jest.clearAllMocks();
  });

  it("renders language selector", async () => {
    const mockLanguages = [
      { languageCode: "en", displayNameEn: "English", isActive: true },
      { languageCode: "hi", displayNameEn: "Hindi", isActive: true },
    ];

    (LocalizationApi.getSupportedLanguages as jest.Mock).mockResolvedValue(mockLanguages);

    render(<LanguageSelector />);

    await waitFor(() => {
      expect(screen.getByRole("combobox")).toBeInTheDocument();
    });
  });

  it("loads and displays languages", async () => {
    const mockLanguages = [
      { languageCode: "en", displayNameEn: "English", isActive: true },
      { languageCode: "hi", displayNameEn: "Hindi", isActive: true },
    ];

    (LocalizationApi.getSupportedLanguages as jest.Mock).mockResolvedValue(mockLanguages);

    render(<LanguageSelector />);

    await waitFor(() => {
      expect(screen.getByText("English")).toBeInTheDocument();
      expect(screen.getByText("Hindi")).toBeInTheDocument();
    });
  });
});

