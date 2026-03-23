import Loader from "@/components/Loader";

export default function SplashScreenLayout() {
    return (
        <div id="splash-screen">
            <div className="splash-brand">
                <svg width="33" height="33" viewBox="0 0 33 33" fill="none" xmlns="http://www.w3.org/2000/svg" style={{ flexShrink: 0 }}>
                    <rect x="5" y="5" width="22" height="22" rx="4" fill="#4F46E5" transform="rotate(45 16 16)" />
                </svg>
                <span>ClientControl</span>
            </div>
            <Loader />
        </div>
    )
}