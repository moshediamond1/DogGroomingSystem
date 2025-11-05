# Dog Grooming Client - React TypeScript

A clean, modern React TypeScript frontend for the dog grooming appointment management system.

## 🚀 Features

✅ **User Authentication**
- Login with username and password
- Register new accounts
- JWT token-based authentication
- Auto-redirect to appointments after login

✅ **Appointment Management**
- View all appointments in a table
- Create new appointments
- Edit your own appointments
- Delete your own appointments (not today's)
- Click on any row to see full details

✅ **Smart Filtering**
- Filter by date range (start/end date)
- Filter by customer name
- Clear all filters

✅ **Responsive Design**
- Works on desktop, tablet, and mobile
- Clean, modern UI
- User-friendly interface

✅ **Real-time Pricing**
- Automatic price calculation based on dog size
- Discount badge when 10% off is applied
- Color-coded pricing display

## 📋 Prerequisites

- Node.js 18+ or 20+
- npm or yarn
- Backend API running on `http://localhost:5000`

## 🛠️ Setup

### 1. Install Dependencies

```bash
npm install
```

### 2. Start Development Server

```bash
npm run dev
```

The app will be available at `http://localhost:3000`

### 3. Make Sure Backend is Running

Ensure your .NET backend is running on `http://localhost:5000` before using the client.

## 📁 Project Structure

```
src/
├── components/          # Reusable components
│   ├── AppointmentModal.tsx
│   ├── AppointmentDetailsModal.tsx
│   ├── AppointmentFilters.tsx
│   └── ProtectedRoute.tsx
├── context/            # React Context (Auth)
│   └── AuthContext.tsx
├── pages/              # Page components
│   ├── Login.tsx
│   ├── Register.tsx
│   └── AppointmentList.tsx
├── services/           # API services
│   └── api.ts
├── types/              # TypeScript types
│   └── index.ts
├── App.tsx             # Main app component
├── main.tsx            # Entry point
└── styles.css          # Global styles
```

## 🎯 User Flow

1. **Landing** → Redirects to `/appointments`
2. **Not Authenticated** → Redirects to `/login`
3. **Login/Register** → Enter credentials → Redirect to appointments
4. **Appointments Page**:
   - View all appointments
   - Filter by date/name
   - Click row to see details
   - Create new appointment
   - Edit/delete your appointments

## 🔐 Authentication

The app uses JWT tokens stored in `localStorage`:
- Token is automatically added to all API requests
- Token persists across page refreshes
- Logout clears the token

## 🎨 UI Features

### Login/Register Pages
- Clean, centered card design
- Form validation
- Error messages
- Loading states
- Links to switch between pages

### Appointments Page
- Header with welcome message and actions
- Filters section for date range and name search
- Responsive table with all appointment data
- Color-coded discount indicator
- Action buttons (edit/delete) only for your appointments
- Click any row to see full details in modal

### Modals
- Create/Edit Appointment Modal:
  - Date/time picker
  - Dog size dropdown with pricing
  - Save/cancel actions
  
- Details Modal:
  - Full appointment information
  - Booking creation timestamp
  - Formatted dates and times

## 🛡️ Security Features

- Protected routes (redirect to login if not authenticated)
- JWT token authentication
- Users can only edit/delete their own appointments
- Automatic token refresh handling

## 🎨 Styling

- Modern, clean design
- CSS variables for easy theming
- Responsive breakpoints
- Smooth transitions and hover effects
- Accessibility-friendly

## 📱 Responsive Design

- Desktop: Full table view with all features
- Tablet: Optimized layout
- Mobile: Scrollable table, stacked filters

## 🔧 Available Scripts

```bash
# Development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Lint code
npm run lint
```

## 🌐 API Integration

The app connects to the backend API at `http://localhost:5000/api`.

Endpoints used:
- `POST /auth/login` - User login
- `POST /auth/register` - User registration
- `GET /appointments` - Get all appointments (with filters)
- `GET /appointments/:id` - Get appointment details
- `POST /appointments` - Create appointment
- `PUT /appointments/:id` - Update appointment
- `DELETE /appointments/:id` - Delete appointment

## 🎯 Component Overview

### Pages

**Login.tsx**
- Username/password form
- Error handling
- Link to register

**Register.tsx**
- Registration form with first name
- Error handling
- Link to login

**AppointmentList.tsx**
- Main appointments table
- Filtering interface
- Create/edit/delete actions
- Detail view modal

### Components

**AppointmentModal.tsx**
- Create/edit appointment form
- Dog size selector with pricing
- DateTime picker

**AppointmentDetailsModal.tsx**
- Read-only appointment details
- Formatted dates and pricing
- Discount information

**AppointmentFilters.tsx**
- Date range inputs
- Customer name search
- Apply/clear actions

**ProtectedRoute.tsx**
- Route guard for authenticated pages

### Context

**AuthContext.tsx**
- Global authentication state
- Login/register/logout functions
- Token management

### Services

**api.ts**
- Axios configuration
- API service functions
- Automatic token injection

## 💡 Tips

1. **Testing Authentication**: Create a user and try logging in/out
2. **Testing Discounts**: Create 4+ appointments to see the 10% discount
3. **Testing Filters**: Use date pickers and name search to filter results
4. **Testing Edit/Delete**: You can only modify your own appointments
5. **Mobile Testing**: Resize browser to see responsive design

## 🐛 Troubleshooting

**Can't connect to API**
- Make sure backend is running on port 5000
- Check browser console for errors
- Verify CORS is enabled on backend

**Token expired**
- Logout and login again
- Token expires after 24 hours

**Filters not working**
- Click "Apply Filters" button
- Date format should be YYYY-MM-DD

## 🚀 Production Build

```bash
npm run build
```

This creates an optimized production build in the `dist/` folder.

To preview:
```bash
npm run preview
```

## 📦 Dependencies

- **React 18** - UI library
- **React Router 6** - Routing
- **TypeScript** - Type safety
- **Axios** - HTTP client
- **date-fns** - Date formatting
- **Vite** - Build tool

## 🎓 Learning Resources

- React: https://react.dev
- TypeScript: https://www.typescriptlang.org
- React Router: https://reactrouter.com
- Vite: https://vitejs.dev

## 📄 License

Created for educational purposes.
